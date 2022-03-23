using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using TRX_Merger.TrxModel;

namespace TRX_Merger.ReportModel
{
    public class UnitTestResultReport
    {
        public UnitTestResultReport(UnitTestResult result)
        {
            Result = result;

            if (!string.IsNullOrEmpty(Result.Output.StdOut))
            {
                if (Result.Output.StdOut.Contains("-> done")
                    || Result.Output.StdOut.Contains("-> error")
                    || Result.Output.StdOut.Contains("-> skipped"))
                {
                    //set cucumber output
                    var rows = Result.Output.StdOut.Split(Environment.NewLine);
                    for (int i = 1; i < rows.Length; i++)
                    {
                        if (rows[i].StartsWith("-> done"))
                        {
                            CucumberStdOut.Add(new KeyValuePair<string, string>(rows[i - 1], "success"));
                            CucumberStdOut.Add(new KeyValuePair<string, string>(rows[i], "success"));
                        }


                        else if (rows[i].StartsWith("-> error"))
                        {
                            CucumberStdOut.Add(new KeyValuePair<string, string>(rows[i - 1], "danger"));
                            CucumberStdOut.Add(new KeyValuePair<string, string>(rows[i], "danger"));
                        }
                        else if (rows[i].StartsWith("-> skipped"))
                        {
                            CucumberStdOut.Add(new KeyValuePair<string, string>(rows[i - 1], "warning"));
                            CucumberStdOut.Add(new KeyValuePair<string, string>(rows[i], "warning"));
                        }
                    }
                }
                else
                {
                    //set standard output
                    StdOutRows = Result.Output.StdOut.Split(Environment.NewLine).ToList();
                }
            }

            if (!string.IsNullOrEmpty(Result.Output.StdErr))
            {
                StdErrRows = Result.Output.StdErr.Split(Environment.NewLine).ToList();
            }

            if (result.Output.ErrorInfo != null)
            {
                if (!string.IsNullOrEmpty(Result.Output.ErrorInfo.Message))
                {
                    //set MessageRows
                    ErrorMessageRows = Result.Output.ErrorInfo.Message.Split(Environment.NewLine).ToList();
                }

                if (!string.IsNullOrEmpty(Result.Output.ErrorInfo.StackTrace))
                {
                    //set StackTraceRows
                    ErrorStackTraceRows = Result.Output.ErrorInfo.StackTrace.Split(Environment.NewLine).ToList();
                }
            }

            ErrorImage = null;
        }


        public string TestId
        {
            get
            {
                var strings = ClassName.Split('.').ToList();
                strings.Add(Result.TestName);

                var id = new StringBuilder();
                strings.ForEach(s => id.Append(s));
                return id.ToString();
            }
        }

        public List<KeyValuePair<string, string>> CucumberStdOut { get; } = new();

        public List<string> StdOutRows { get; set; }

        public List<string> StdErrRows { get; set; }

        public List<string> ErrorMessageRows { get; set; }

        public List<string> ErrorStackTraceRows { get; set; }

        public string AsJson() => JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });

        public string FormattedStartTime => DateTime.Parse(Result.StartTime).ToString("G");

        public string FormattedEndTime => DateTime.Parse(Result.EndTime).ToString("G");

        public string FormattedDuration => TimeSpan.Parse(Result.Duration).TotalSeconds.ToString("n2") + " sec.";

        public UnitTestResult Result { get; set; }

        public string Dll { get; set; }

        public string ClassName { get; set; }

        public string ErrorImage { get; set; }
    }
}