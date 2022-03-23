using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TRX_Merger.ReportGenerator;

namespace TRX_Merger
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length == 0
                || args.Contains("/h")
                || args.Contains("/help"))
            {
                DispalyHelp();
                return 1;
            }

            if (args.FirstOrDefault(a => a.StartsWith("/trx")) == null)
            {
                Console.WriteLine("/trx parameter is required");
                return 1;
            }

            string trxArg = args.FirstOrDefault(a => a.StartsWith("/trx"));
            var trxFiles = ResolveTrxFilePaths(trxArg, args.Contains("/r"));
            if (trxFiles.Count == 0)
            {
                Console.WriteLine("No trx files found!");
                return 1;
            }

            if (trxFiles.Count == 1)
            {
                if (trxFiles[0].StartsWith("Error: "))
                {
                    Console.WriteLine(trxFiles[0]);
                    return 1;
                }

                if (args.FirstOrDefault(a => a.StartsWith("/report")) == null)
                {
                    Console.WriteLine("Error: Only one trx file has been passed and there is no /report parameter. When having only one trx in /trx argument, /report parameter is required.");
                    return 1;
                }

                if (args.FirstOrDefault(a => a.StartsWith("/output")) != null)
                {
                    Console.WriteLine("Error: /output parameter is not allowed when having only one trx in /trx argument!.");
                    return 1;
                }

                string reportParam = args.FirstOrDefault(a => a.StartsWith("/report"));
                if (reportParam != null)
                {
                    string reportOutput = ResolveReportLocation(reportParam);
                    if (reportOutput.StartsWith("Error: "))
                    {
                        Console.WriteLine(trxFiles[0]);
                        return 1;
                    }

                    string screenshotLocation = ResolveScreenshotLocation(args.FirstOrDefault(a => a.StartsWith("/screenshots")));
                    string reportTitle = ResolveReportTitle(args.FirstOrDefault(a => a.StartsWith("/reportTitle")));
                    try
                    {
                        TrxReportGenerator.GenerateReport(trxFiles[0], reportOutput, screenshotLocation, reportTitle);
                    }
                    catch (Exception ex)
                    {
                        while (ex.InnerException != null)
                            ex = ex.InnerException;

                        Console.WriteLine("Error: " + ex.Message);
                        return 1;
                    }
                }
                else
                {
                    // Report parameter is not supplied, a summary is printed in console
                    return TrxReportGenerator.GenerateSummaryOnConsole(trxFiles[0]);
                }
            }
            else
            {
                if (args.FirstOrDefault(a => a.StartsWith("/output")) == null)
                {
                    Console.WriteLine("/output parameter is required, when there are multiple trx files in /trx argument");
                    return 1;
                }

                string outputParam = ResolveOutputFileName(args.FirstOrDefault(a => a.StartsWith("/output")));
                if (outputParam.StartsWith("Error: "))
                {
                    Console.WriteLine(outputParam);
                    return 1;
                }

                if (trxFiles.Contains(outputParam))
                    trxFiles.Remove(outputParam);

                try
                {
                    var combinedTestRun = TestRunMerger.MergeTRXsAndSave(trxFiles, outputParam);

                    string reportOutput = ResolveReportLocation(args.FirstOrDefault(a => a.StartsWith("/report")));
                    if (reportOutput == null)
                    {
                        // Report parameter is not supplied, a summary is printed in console
                        return TrxReportGenerator.GenerateSummaryOnConsole(combinedTestRun);
                    }

                    if (reportOutput.StartsWith("Error: "))
                    {
                        Console.WriteLine(trxFiles[0]);
                        return 1;
                    }

                    string screenshotLocation = ResolveScreenshotLocation(args.FirstOrDefault(a => a.StartsWith("/screenshots")));
                    string reportTitle = ResolveReportTitle(args.FirstOrDefault(a => a.StartsWith("/reportTitle", StringComparison.CurrentCultureIgnoreCase)));

                    TrxReportGenerator.GenerateReport(combinedTestRun, reportOutput, screenshotLocation, reportTitle);
                }
                catch (Exception ex)
                {
                  Console.WriteLine(ex);
                    while (ex.InnerException != null)
                        ex = ex.InnerException;

                    Console.WriteLine("Error: " + ex.Message);
                    return 1;
                }
            }

            return 0;
        }

        private static void DispalyHelp()
        {
            Console.WriteLine(
            @"
PARAMETERS:

/trx - parameter that determines which trx files will be merged. REQUIRED PARAMETER
	This parameter will accept one of the following:
		- file(s) name: looks for trx files in the current directory.File extension is required
			example: /trx:testResults1.trx,testResults2.trx,testResults3.trx
		- file(s) path: full path to trx files.File extension is required
			example: /trx:c:\TestResults\testResults1.trx,c:\TestResults\testResults2.trx,c:\TestResults\testResults3.trx
		- directory(s): directory containing trx files. it gets all trx files in the directory
			example: /trx:c:\TestResults,c:\TestResults1
		- empty: gets all trx files in the current directory
			example: /trx
        - combination: you can pass files and directories at the same time:
            example: /trx:c:\TestResults,c:\TestResults1\testResults2.trx

/output - the name of the output trx file. File extension is required. REQIRED if more than one trx file is defined in the /trx parameter. If only one trx is present in /trx this parameter should not be passed.
	- name: saves the file in the current directory
		example: /output:combinedTestResults.trx
	- path and name: saves the file in specified directory.
		example: /output:c:\TestResults\combinedTestResults.trx

/r - recursive search in directories.OPTIONAL PARAMETER.\nWhen there is a directory in /trx param (ex: /trx:c:\TestResuts), and this parameter is passed, the rearch for trx files will be recursive
    example: /trx:c:\TestResults,c:\TestResults1\testResults2.trx /r /output:combinedTestResults.trx

/report - generates a html report from a trx file. REQUIRED if one trx is specified in /trx parameter and OPTIONAL otherwise.\n If one trx is passed to the utility, the report is for it, otherwise, the report is generated for the /output result
    - fill path to where the report should be saved. including the name of the file and extension.
    example /report:c:\Tests\report.html

/screenshots - path to a folder which contains screenshots corresponding to failing tests. OPTIONAL PARAMETER
    - in order a screenshot to be shown in the report for a given test, the screenshto should contain the name of the test method.
            ");
        }

        private static string ResolveOutputFileName(string outputParam)
        {
            var splitOutput = outputParam.Split(new char[] { ':' });

            if (splitOutput.Length == 1
                || !outputParam.EndsWith(".trx"))
                return "Error: /output parameter is in the incorrect format. Expected /output:<file name | directory and file name>. Execute /help for more information";

            return outputParam[8..];
        }

        private static string ResolveReportLocation(string reportParam)
        {
            if (string.IsNullOrEmpty(reportParam))
                return null;

            var splitReport = reportParam.Split(new char[] { ':' });

            if (splitReport.Length == 1
                || !reportParam.EndsWith(".html"))
                return "Error: /report parameter is in the correct format. Expected /report:<file name | directory and file name>. Execute /help for more information";

            return reportParam[8..];
        }

        private static string ResolveScreenshotLocation(string screenshots)
        {
            if (string.IsNullOrEmpty(screenshots))
                return null;

            var splitScreenshots = screenshots.Split(new char[] { ':' });

            if (splitScreenshots.Length == 1)
                return "Error: /screenshots parameter is in the correct format. Expected /screenshots:<directory name>. Execute /help for more information";

            var screenshotsLocation = screenshots[13..];
            if (!Directory.Exists(screenshotsLocation))
                return "Error: Folder: " + screenshotsLocation + "does not exists";

            return screenshotsLocation;
        }

        private static string ResolveReportTitle(string reportTitle)
        {
            if (string.IsNullOrEmpty(reportTitle))
                return null;

            var splitScreenshots = reportTitle.Split(new char[] { ':' });

            if (splitScreenshots.Length == 1)
                return "Error: /reportTitle parameter is in the correct format. Expected /reportTitle:<title>. Execute /help for more information";

            var screenshotsLocation = reportTitle[("/reportTitle".Length + 1)..];

            return screenshotsLocation;
        }

        private static List<string> ResolveTrxFilePaths(string trxParams, bool recursive)
        {
            var searchOpts = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            
            if (trxParams == "/trx")
                return Directory.GetFiles(Directory.GetCurrentDirectory(), "*.trx", searchOpts).ToList();

            List<string> paths = new();

            var args = trxParams[5..].Split(',').ToList();

            foreach (var a in args)
            {
                bool isTrxFile = File.Exists(a) && a.EndsWith(".trx");
                bool isDir = Directory.Exists(a);

                if (!isTrxFile && !isDir)
                    return new List<string> { $"Error: {a} is not a trx file or directory" };

                if (isTrxFile)
                    paths.Add(a);

                if (isDir)
                    paths.AddRange(Directory.GetFiles(a, "*.trx", searchOpts).ToList());
            }

            return paths;
        }
    }
}
