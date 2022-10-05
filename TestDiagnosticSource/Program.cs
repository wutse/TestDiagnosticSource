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
        private static NLog.Logger logger;

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

            var logFactory = new LogFactory();
            var logConfig = new NLog.Config.XmlLoggingConfiguration("NLog.config", logFactory);
            logFactory.Configuration = logConfig;

            logger = logFactory.GetCurrentClassLogger();


            await DoSomeWork("banana", 8);
            Console.WriteLine("Example work done");
        }

        // All the functions below simulate doing some arbitrary work
        static async Task DoSomeWork(string foo, int bar)
        {

            //using (var newActivity = new System.Diagnostics.Activity("MyOperation").Start())
            //{
            //    //var dateTime = DateTime.UtcNow.Date;
            //    //newActivity.SetStartTime(dateTime);
            //    //newActivity.SetEndTime(dateTime.AddHours(1).AddMinutes(2).AddSeconds(3).AddTicks(40567));
            //    logger.Info("Hello");
            //}

            //using (Activity activity = source.StartActivity("DoSomeWork"))
            using (var newActivity = new System.Diagnostics.Activity("DoSomeWork").Start())
            {
                await StepOne();
                await StepTwo();
                logger.Info("DoSomeWork");
            }
        }

        static async Task StepOne()
        {
            //using (Activity activity = source.StartActivity("StepOne"))
            using (var newActivity = new System.Diagnostics.Activity("StepOne").Start())
            {
                await Task.Delay(500);
                logger.Info("StepOne");
            }
        }

        static async Task StepTwo()
        {
            //using (Activity activity = source.StartActivity("StepTwo"))
            using (var newActivity = new System.Diagnostics.Activity("StepTwo").Start())
            {
                await Task.Delay(1000);
                logger.Info("StepTwo");
            }
        }
    }
}