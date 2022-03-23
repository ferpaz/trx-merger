using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TRX_Merger.TrxModel;

namespace TRX_Merger.ReportModel
{
    public class TestRunReport
    {
        public TestRunReport(TestRun run)
        {
            Run = run;

            TestClasses = Run.TestDefinitions.Select(td => td.TestMethod.ClassName).Distinct().ToList();

            TestClassReports = new Dictionary<string, TestClassReport>();
            Parallel.ForEach(TestClasses, testClass => TestClassReports.Add(testClass, GetTestClassReport(testClass)));
            
            AllFailedTests = TestClassReports.SelectMany(t => t.Value.Tests.Where(r => r.Result.Outcome != "Passed" && r.Result.Outcome != "NotExecuted")).ToList();
            AllNotExecutedTests = TestClassReports.SelectMany(t => t.Value.Tests.Where(r => r.Result.Outcome == "NotExecuted")).ToList();
        }

        public TestRun Run { get; set; }

        public List<string> TestClasses { get; }

        public List<UnitTestResultReport> AllFailedTests { get; }

        public List<UnitTestResultReport> AllNotExecutedTests { get; }

        public Dictionary<string, TestClassReport> TestClassReports { get; }

        public string TestClassReportsJson()
        {
            var test = System.Text.Json.JsonSerializer.Serialize(
                TestClassReports
                .Select(s => s.Value)
                .Select(c => new
                {
                    ClassName = c.FriendlyTestClassName,
                    c.Passed,
                    c.NotExecuted,
                    c.Failed,
                    c.Timeout,
                    c.Aborted
                })
                .ToList());
            return test;
        }

        public TestClassReport GetTestClassReport(string className)
        {
            var tests = Run.TestDefinitions.Where(td => td.TestMethod.ClassName.EndsWith(className)).Select(ttdd => ttdd.TestMethod.Name).ToList();
            var results = Run.Results.Where(r => tests.Contains(r.TestName)).ToList();

            List<UnitTestResultReport> resultReports = new();

            results.ForEach(r =>
                resultReports.Add(
                    new UnitTestResultReport(r)
                    {
                        ClassName = className,
                        Dll = Run.TestDefinitions.FirstOrDefault(d => d.Name == r.TestName).TestMethod.CodeBase
                    }));

            return new TestClassReport(className, resultReports);
        }
    }
}
