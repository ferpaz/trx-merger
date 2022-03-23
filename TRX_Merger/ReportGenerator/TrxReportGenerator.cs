using System;
using System.IO;
using System.Linq;
using RazorEngine;
using RazorEngine.Templating;
using TRX_Merger.ReportModel;
using TRX_Merger.TrxModel;
using TRX_Merger.Utilities;

namespace TRX_Merger.ReportGenerator
{
    public static class TrxReportGenerator
    {
        public static void GenerateReport(string trxFilePath, string outputFile, string screenshotLocation, string reportTitle)
        {
            var testRun = TrxSerializationUtils.DeserializeTRX(trxFilePath);

            GenerateReport(testRun, outputFile, screenshotLocation, reportTitle);
        }

        public static void GenerateReport(TestRun run, string outputFile, string screenshotLocation, string reportTitle)
        {
            if (!string.IsNullOrEmpty(reportTitle))
                run.Name = reportTitle;

            Console.WriteLine("Generating HTML Report");
            string template = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ReportGenerator/trx_report_template.html"));

            TestRunReport model = new(run);

            string result = Engine.Razor.RunCompile(
                template,
                "rawTemplate",
                null,
                model);

            //TODO: Implement screenshot logic here!

            if (File.Exists(outputFile))
            {
                Console.WriteLine("Deleting: " + outputFile);
                File.Delete(outputFile);
            }

            File.WriteAllText(outputFile, result);
        }

        public static int GenerateSummaryOnConsole(string trxFilename)
        {
            return GenerateSummaryOnConsole(TrxSerializationUtils.DeserializeTRX(trxFilename));
        }

        public static int GenerateSummaryOnConsole(TestRun testRun)
        {
            Console.WriteLine("Generating summary report");
            Console.WriteLine();

            TestRunReport Model = new(testRun);

            var counters = Model.Run.ResultSummary.Counters;

            Console.WriteLine("----------------------------------------------------------------------------");
            Console.WriteLine("Test Run Summary");
            Console.WriteLine("----------------------------------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine($"      Started At: {DateTime.Parse(Model.Run.Times.Start):G}");
            Console.WriteLine($"       Finish At: {DateTime.Parse(Model.Run.Times.Finish):G}");
            Console.WriteLine($"        Duration: {Model.Run.Duration}");
            Console.WriteLine();
            Console.WriteLine($"Test Run Outcome: {(counters.Failed > 0 ? Model.Run.ResultSummary.Outcome.ToUpper() : "PASSED")}");
            Console.WriteLine();
            Console.WriteLine("----------------------------------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine($"          Passed: {counters.Passed:n0}");
            Console.WriteLine();
            Console.WriteLine($"    Not Executed: {(counters.NotExecuted != 0 ? counters.NotExecuted : counters.Total - counters.Passed - counters.Failed - counters.Timeout - counters.Aborted):n0}");
            Console.WriteLine();
            Console.WriteLine($"          Failed: {counters.Failed:n0}");
            Console.WriteLine($"         Timeout: {counters.Timeout:n0}");
            Console.WriteLine($"         Aborted: {counters.Aborted:n0}");
            Console.WriteLine();
            Console.WriteLine($"           TOTAL: {counters.Total:n0}");
            Console.WriteLine();
            Console.WriteLine("----------------------------------------------------------------------------");

            if (Model.AllFailedTests.Any())
            {
                Console.WriteLine("FAILED TESTS");
                foreach (var test in Model.AllFailedTests.OrderBy(t => t.Dll).ThenBy(t => t.ClassName).ThenBy(t => t.Result.TestName))
                {
                    Console.WriteLine("----------------------------------------------------------------------------");
                    Console.WriteLine();
                    Console.WriteLine(test.Dll);
                    Console.WriteLine($"  {test.ClassName}");
                    Console.WriteLine($"    {test.Result.TestName}");
                    Console.WriteLine($"    {test.Result.Outcome.ToUpper()} - {DateTime.Parse(test.Result.StartTime):G} ({TimeSpan.Parse(test.Result.Duration).TotalSeconds:n2} sec)");
                    Console.WriteLine();
                    Console.WriteLine(test.Result.Output.ErrorInfo.Message);
                    Console.WriteLine();
                    Console.WriteLine(test.Result.Output.ErrorInfo.StackTrace);
                    Console.WriteLine();
                }
                Console.WriteLine("----------------------------------------------------------------------------");
            }

            if (Model.AllNotExecutedTests.Any())
            {
                Console.WriteLine("NOT EXECUTED TESTS (Skipped)");
                foreach (var test in Model.AllNotExecutedTests.OrderBy(t => t.Dll).ThenBy(t => t.ClassName).ThenBy(t => t.Result.TestName))
                {
                    Console.WriteLine("----------------------------------------------------------------------------");
                    Console.WriteLine();
                    Console.WriteLine(test.Dll);
                    Console.WriteLine($"  {test.ClassName}");
                    Console.WriteLine($"    {test.Result.TestName}");
                    Console.WriteLine($"    {test.Result.Outcome.ToUpper()} - {DateTime.Parse(test.Result.StartTime):G} ({TimeSpan.Parse(test.Result.Duration).TotalSeconds:n2} sec)");
                    Console.WriteLine();
                    if (test.Result.Output.ErrorInfo?.Message != null)
                    {
                        Console.WriteLine(test.Result.Output.ErrorInfo.Message);
                        Console.WriteLine();
                    }
                }
            }

            Console.WriteLine();
            return counters.Failed > 0 ? 1 : 0;
        }
    }
}
