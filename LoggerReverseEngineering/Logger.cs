namespace Sommer.ReverseEngineeringTestLogger
{
    using System.Collections.Generic;
    using System.IO;
    using log4net;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
    using Newtonsoft.Json;

    [FriendlyName(FriendlyName)]
    [ExtensionUri(ExtensionUri)]
    public class Logger : ITestLoggerWithParameters
    {
        public const string ExtensionUri = "logger://Microsoft/TestPlatform/ReversingLogger/v1";
        public const string FriendlyName = "reverse";
        private readonly ILog logger;

        public Logger()
        {
            var path = Path.GetDirectoryName(typeof(Logger).Assembly.Location);
            log4net.Config.XmlConfigurator.Configure(File.Open(Path.Combine(path, "log4net.config"), FileMode.Open));
            this.logger = log4net.LogManager.GetLogger(typeof(Logger));
        }

        public void Initialize(TestLoggerEvents events, Dictionary<string, string> parameters)
        {
            this.logger.Debug($"Initialize :: \r\nEvents: {JsonConvert.SerializeObject(events)}\r\nParameters: {JsonConvert.SerializeObject(parameters)}");
            parameters.TryGetValue("TestRunDirectory", out var testRunDirectory);
            Setup(events, testRunDirectory);
        }

        public void Initialize(TestLoggerEvents events, string testRunDirectory)
        {
            this.logger.Debug($"Initialize :: \r\nEvents: {JsonConvert.SerializeObject(events)}\r\nTest Run Directory: {testRunDirectory}");
            Setup(events, testRunDirectory);
        }

        private void Setup(TestLoggerEvents events, string testRunDirectory)
        {
            events.TestRunMessage += (s, e) =>
            {
                this.logger.Debug($"TestRunMessage :: \r\nEvent: {JsonConvert.SerializeObject(e)}");
            };
            events.TestResult += (sender, evt) =>
            {
                this.logger.Debug($"TestResult :: \r\nEvent: {JsonConvert.SerializeObject(evt)}");
            };
            events.TestRunComplete += (sender, evt) =>
            {
                this.logger.Debug($"TestRunComplete :: \r\nEvent: {JsonConvert.SerializeObject(evt)}");
            };
        }
    }
}
