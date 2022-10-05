// See https://aka.ms/new-console-template for more information
using NLog.LayoutRenderers;
using NLog;
using OpenTelemetry;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace Sample.DistributedTracing
{
    class Program
    {
        private static ActivitySource source = new ActivitySource("Sample.DistributedTracing", "1.0.0");

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            //dotnet add package OpenTelemetry.Exporter.Console
            /*
            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MySample"))
                .AddSource("Sample.DistributedTracing")
                .AddConsoleExporter()
                .Build();
            */

            await DoSomeWork("banana", 8);
            Console.WriteLine("Example work done");
        }

        // All the functions below simulate doing some arbitrary work
        static async Task DoSomeWork(string foo, int bar)
        {
            string inputLayout = @"${message}|ActivityId=${activity:property=TraceId}";
            string expectedResult = "3723004.056";

            var logFactory = new LogFactory();
            //var xmlStream = new System.IO.StringReader($@"<nlog throwConfigExceptions='true'>
            //    <targets>
            //        <target name='Memory' type='Memory' layout='{inputLayout}' />
            //    </targets>
            //    <rules>
            //        <logger name='*' writeTo='Memory' />
            //    </rules></nlog>");
            //var xmlReader = System.Xml.XmlReader.Create(xmlStream);
            var logConfig = new NLog.Config.XmlLoggingConfiguration("NLog.config", logFactory);
            logFactory.Configuration = logConfig;

            var logger = logFactory.GetCurrentClassLogger();
            using (var newActivity = new System.Diagnostics.Activity("MyOperation").Start())
            {
                var dateTime = DateTime.UtcNow.Date;
                newActivity.SetStartTime(dateTime);
                newActivity.SetEndTime(dateTime.AddHours(1).AddMinutes(2).AddSeconds(3).AddTicks(40567));
                logger.Info("Hello");
            }

            using (Activity activity = source.StartActivity("SomeWork"))
            {
                await StepOne();
                await StepTwo();
            }
        }

        static async Task StepOne()
        {
            using (Activity activity = source.StartActivity("StepOne"))
            {
                await Task.Delay(500);
            }
        }

        static async Task StepTwo()
        {
            using (Activity activity = source.StartActivity("StepTwo"))
            {
                await Task.Delay(1000);
            }
        }
    }
}