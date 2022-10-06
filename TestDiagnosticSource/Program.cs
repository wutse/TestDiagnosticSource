// See https://aka.ms/new-console-template for more information
using NLog;
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
            TestClass testClass = new TestClass(logger);

            Task task1 =  DoSomeWork1("banana", 8);
            Task task2 = testClass.DoSomeWork2("Apple", 1);

            Task.WaitAll(task1, task2);

            Console.WriteLine("Example work done");
        }

        // All the functions below simulate doing some arbitrary work
        static async Task DoSomeWork1(string foo, int bar)
        {

            //using (var newActivity = new System.Diagnostics.Activity("MyOperation").Start())
            //{
            //    //var dateTime = DateTime.UtcNow.Date;
            //    //newActivity.SetStartTime(dateTime);
            //    //newActivity.SetEndTime(dateTime.AddHours(1).AddMinutes(2).AddSeconds(3).AddTicks(40567));
            //    logger.Info("Hello");
            //}

            //using (Activity activity = source.StartActivity("DoSomeWork"))
            using (var newActivity = new System.Diagnostics.Activity("DoSomeWork1").Start())
            {
                await StepOne1();
                await StepTwo1();
                logger.Info("DoSomeWork1 End");
            }
        }

        static async Task StepOne1()
        {
            //using (Activity activity = source.StartActivity("StepOne"))
            using (var newActivity = new System.Diagnostics.Activity("StepOne1").Start())
            {
                await Task.Delay(500);
                logger.Info("StepOne1 End");
            }
        }

        static async Task StepTwo1()
        {
            //using (Activity activity = source.StartActivity("StepTwo"))
            using (var newActivity = new System.Diagnostics.Activity("StepTwo1").Start())
            {
                await Task.Delay(1000);
                logger.Info("StepTwo1 End");
            }
        }

    }

    class TestClass
    {
        private NLog.Logger logger;

        public TestClass(NLog.Logger logger)
        {
            this.logger = logger;
        }

        // All the functions below simulate doing some arbitrary work
        internal async Task DoSomeWork2(string foo, int bar)
        {

            using (var newActivity = new System.Diagnostics.Activity("DoSomeWork2").Start())
            {
                await StepOne2();
                await StepTwo2();
                logger.Info("DoSomeWork2 End");
            }
        }

        async Task StepOne2()
        {
            await Task.Delay(500);
            logger.Info("StepOne2 End");
        }

        async Task StepTwo2()
        {
            await Task.Delay(1000);
            logger.Info("StepTwo2 End");
        }
    }
}