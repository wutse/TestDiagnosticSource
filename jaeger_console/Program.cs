// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Instrumentation.Runtime;
using System.Data;


namespace jaegerTest.ConsoleProgram
{
    public class Program
    {
        private static readonly ActivitySource Activity = new(nameof(Program));
        private static readonly TextMapPropagator Propagator = new TraceContextPropagator();

        private static IConfiguration _configuration;
        private static ILogger<Program> _logger;
        public static async Task Main()
        {
            try
            {

                SetupConfiguration();
                SetupLogger();
                using var openTelemetry = SetupOpenTelemetry();
                _logger.LogInformation("Process Main start: ");


                Parallel.For(0, 1000, async i =>
                {
                    using (var activity = Activity.StartActivity("Process Main Parallel", ActivityKind.Server))
                    {
                        ActivityContext nowContext;
                        ActivityContext nowContext2;
                        activity?.SetTag("Method", "ParallelFor");
                        DoWork(activity.Context, out nowContext);
                        DoWork2(nowContext, out nowContext2);
                        var rtn = await DoWorkAsync(i);

                        _logger.LogInformation("Process Main End: "+i);

                    }
                });

                for(var i= 0; i < 1000;i++)
                {
                    using (var activity = Activity.StartActivity("Process Main", ActivityKind.Server))
                    {
                        ActivityContext nowContext;
                        ActivityContext nowContext2;
                        activity?.SetTag("Method", "For");
                        DoWork(activity.Context, out nowContext);
                        DoWork2(nowContext, out nowContext2);
                        var rtn = await DoWorkAsync(i);


                        _logger.LogInformation("Process Main End: "+i);

                    }
                }

                Console.WriteLine("Hello, World!");
                System.Console.WriteLine(" Press [enter] to exit.");
                System.Console.ReadLine();

            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
                throw;
            }

        }
        public static void DoWork(ActivityContext parentActivityContext,out ActivityContext outContext)
        {
            using (var activity = Activity.StartActivity("Process Message", ActivityKind.Internal, parentActivityContext))
            {

                //AddActivityTags(activity);

                _logger.LogInformation("Message Received: ");
                outContext = activity.Context;
            }

        }
        public static void DoWork2(ActivityContext parentActivityContext ,out ActivityContext outContext)
        {

            using (var activity = Activity.StartActivity("Process Message2",ActivityKind.Consumer, parentActivityContext))
            {

                //AddActivityTags(activity);

                _logger.LogInformation("Message Received2: ");
                outContext = activity.Context;
            }
            

        }
        public static async Task<string> DoWorkAsync(int i)
        {

            using (var activity = Activity.StartActivity("Process Message Async", ActivityKind.Consumer, System.Diagnostics.Activity.Current.Context))
            {
                System.Diagnostics.Activity.Current.SetTag("i", i);
                _logger.LogInformation("Message Received DoWorkAsync: "+i);
            }
                return await Task.FromResult<string>(DateTime.Now.ToString());
        }
        private static void AddActivityTags(Activity activity)
        {
            activity?.SetTag("messaging.system", "rabbitmq");
            activity?.SetTag("messaging.destination_kind", "queue");
            activity?.SetTag("messaging.rabbitmq.queue", "sample");
        }
        private static void SetupConfiguration()
        {

            //setup config
            var configFiles = Directory
                .GetFiles(Path.Combine(Directory.GetCurrentDirectory()),
                    "appsettings.json").ToList();

            if (!configFiles.Any())
                throw new Exception("Cannot read config file");

            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(configFiles[0], true, false)
                .AddEnvironmentVariables()
                .Build();
        }

        private static void SetupLogger()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                    .AddConsole();
            });

            _logger = loggerFactory.CreateLogger<Program>();
        }

        private static TracerProvider SetupOpenTelemetry()
        {
            return Sdk.CreateTracerProviderBuilder()
                .AddHttpClientInstrumentation()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("App2"))
                .AddSource(nameof(Program))
                .AddJaegerExporter(opts =>
                {
                    opts.AgentHost = _configuration["Jaeger:AgentHost"];
                    opts.AgentPort = Convert.ToInt32(_configuration["Jaeger:AgentPort"]);
                    opts.ExportProcessorType = ExportProcessorType.Simple;
                })
                .Build();
        }
    } 
}
