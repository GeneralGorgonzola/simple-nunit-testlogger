namespace GeneralGorgonzola.TestLogger
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

    [FriendlyName("nunit2")]
    [ExtensionUri("logger://Microsoft/TestPlatform/Nunit2Logger/v1")]
    public class Logger : ITestLoggerWithParameters
    {
        public void Initialize(TestLoggerEvents events, Dictionary<string, string> parameters)
        {
            parameters.TryGetValue("TestRunDirectory", out var testRunDirectory);
            parameters.TryGetValue("LogFilePath", out var logFilePath);
            parameters.TryGetValue("Suite", out var suiteName);
            Setup(events, testRunDirectory, suiteName, logFilePath);
        }

        public void Initialize(TestLoggerEvents events, string testRunDirectory)
        {
            Setup(events, testRunDirectory);
        }

        private void Setup(TestLoggerEvents events, string testRunDirectory, string suiteName = null, string logFilePath = null)
        {
            var testResults = new List<TestResult>();
            var testEventLock = new object();

            events.TestResult += (sender, evt) =>
            {
                var result = evt.Result;
                lock(testEventLock)
                {
                    testResults.Add(result);
                }
                
            };
            events.TestRunComplete += (sender, evt) =>
            {
                lock(testEventLock)
                {
                    if (testResults.Any())
                    {
                        var currentCulture = Thread.CurrentThread.CurrentCulture;
                        try
                        {
                            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                            var root = PrepareTestReport(evt, suiteName, testResults);
                            SaveTestResults(logFilePath, testRunDirectory, root);
                        }
                        finally
                        {
                            Thread.CurrentThread.CurrentCulture = currentCulture;
                        }
                    }
                }
            };
        }

        private void SaveTestResults(string logFilePath, string testRunDirectory, Zool.ResultType root)
        {
            var pathToResultsFile = logFilePath ?? root.Name + "_TestResult.xml";
            var tool = new XmlTool();
            tool.SaveXML(pathToResultsFile, root);
        }

        private Zool.ResultType PrepareTestReport(TestRunCompleteEventArgs evt, string suiteName, IEnumerable<TestResult> testResults)
        {
            suiteName = suiteName ?? Path.GetFileNameWithoutExtension(testResults.First().TestCase.Source);
            var root = new Zool.ResultType()
            {
                Name = suiteName,
                Total = evt.TestRunStatistics.ExecutedTests,
                Errors = evt.TestRunStatistics[TestOutcome.Failed],
                Failures = evt.TestRunStatistics[TestOutcome.Failed],
                Not_Run = evt.TestRunStatistics[TestOutcome.Skipped],
                Inconclusive = evt.TestRunStatistics[TestOutcome.None],
                Ignored = evt.TestRunStatistics[TestOutcome.Skipped],
                Skipped = evt.TestRunStatistics[TestOutcome.Skipped],
                Invalid = evt.TestRunStatistics[TestOutcome.NotFound],
                Date = DateTime.Today.ToShortDateString(),
                Time = evt.ElapsedTimeInRunningTests.ToString(),
                Test_Suite = new Zool.Test_SuiteType()
                {
                    Results = new Zool.ResultsType()
                }
            };

            var suites = GetSuitesGroupedByTestProject(testResults);

            if (suites.Count() > 1)
            {
                foreach(var suite in suites)
                {
                    root.Test_Suite.Results.Test_Suite.Add(suite);
                }
            }
            else
            {
                root.Test_Suite = suites.First();
            }
            return root;
        }

        private IEnumerable<Zool.Test_SuiteType> GetSuitesGroupedByTestProject(IEnumerable<TestResult> testResults)
        {
            var suites = testResults
                .GroupBy(res => res.TestCase.Source)
                .Select(s => 
                {
                    var suiteName = Path.GetFileNameWithoutExtension(s.Key);
                    var suite = new Zool.Test_SuiteType()
                    {
                        Name = suiteName,
                        Executed = true.ToString(),
                        Success = s.All(t => t.Outcome == TestOutcome.Passed || t.Outcome == TestOutcome.Skipped).ToString(),
                        Result = s.All(t => t.Outcome == TestOutcome.Passed || t.Outcome == TestOutcome.Skipped) ? "Success" : "Failure",
                        Time = s.Sum(t => t.Duration.TotalSeconds).ToString(),
                        Results = GetResultsForTestProject(s)                              
                    };
                    return suite;
                });
            return suites;
        }

        private Zool.ResultsType GetResultsForTestProject(IEnumerable<TestResult> resultForTestSource)
        {
            var result = new Zool.ResultsType();
            foreach(var resultsForTestClass in resultForTestSource
                .GroupBy(u => GetClassNameFromFullyQualifiedName(u.TestCase.FullyQualifiedName)))
            {
                var classSuite = new Zool.Test_SuiteType()
                {
                    Name = resultsForTestClass.Key,
                    Results = new Zool.ResultsType()
                };
                var testResultsForTestClass = GetTestResultsForTestClass(resultsForTestClass);
                foreach(var tc in testResultsForTestClass)
                {
                    classSuite.Results.Test_Case.Add(tc);
                }

                result.Test_Suite.Add(classSuite);
            }
            return result;
        }

        private IEnumerable<Zool.Test_CaseType> GetTestResultsForTestClass(IEnumerable<TestResult> classResult)
        {
            foreach(var testresult in classResult)
            {
                var tc = new Zool.Test_CaseType()
                {
                    Name = testresult.DisplayName,
                    Executed = (testresult.Outcome != TestOutcome.None).ToString(),
                    Success = (testresult.Outcome == TestOutcome.Passed).ToString(),
                    Result = testresult.Outcome.ToString(),
                    Time = testresult.Duration.TotalSeconds.ToString()
                };
                if (testresult.Outcome == TestOutcome.Failed)
                {
                    tc.Failure = new Zool.FailureType()
                    {
                        Message = testresult.ErrorMessage,
                        Stack_Trace = testresult.ErrorStackTrace
                    };
                }
                yield return tc;
            }
        }

        string GetClassNameFromFullyQualifiedName(string fullyQualifiedName)
        {
            if (fullyQualifiedName != null)
            {
                var split = fullyQualifiedName.Split('.');
                if (split.Length > 1)
                {
                    fullyQualifiedName = string.Join(".", fullyQualifiedName.Split('.').Reverse().Skip(1).Reverse());
                }
            }
            return fullyQualifiedName;
        }
    }
}
