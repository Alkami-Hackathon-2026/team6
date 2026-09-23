#if NET6_0_OR_GREATER
using Alkami.Broker.App;
using Alkami.Exceptions;
using Alkami.Extensions.DependencyInjection;
using Alkami.Extensions.Logging;
using Alkami.Extensions.RpcHost.Health;
using Alkami.Services.Subscriptions.ParticipatingService.Services;
using Alkami.Utilities.Kubernetes;
using Common.Logging;
using Common.Logging.Configuration;
using CoreWCF.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace Alkami.Services.Subscriptions.ParticipatingService
{
    /// <summary>
    /// The <see cref="AlkamiServiceHost"/> is customized to produce a streamlined host for Alkami Microservice applications
    /// </summary>
    public class AlkamiServiceHost
    {
        private readonly ILog _logger;
        private readonly WebApplicationBuilder _builder;
        private readonly string _friendlyName;

        /// <summary>
        /// The configuration setting for the service path.
        /// </summary>
        public const string ServicePath = "ServicePath";

        /// <summary>
        /// The configuration setting for pulling the service's version.
        /// </summary>
        public const string ServiceVersion = "ServiceVersion";

        /// <summary>
        /// The configuration setting for pulling the package's name.
        /// </summary>
        public const string PackageName = "PackageName";

        /// <summary>
        /// The configuration setting for pulling the package's name.
        /// </summary>
        public const string FriendlyServiceName = "FriendlyServiceName";

        /// <summary>
        /// The configuration for the service port.
        /// </summary>
        public const string ServicePortConfiguration = "ServicePort";

        /// <summary>
        /// The Serilog configuration Path key for overriding the file path.
        /// </summary>
        internal const string SerilogConfigurationPathKey = "Serilog:WriteTo:0:Args:path";

        /// <summary>
        /// The Serilog configuration path key for setting the PackageName property.
        /// </summary>
        internal const string SerilogPackageNameKey = "Serilog:Properties:PackageName";

        /// <summary>
        /// The Serilog configuration path key for setting the Version property.
        /// </summary>
        internal const string SerilogVersionKey = "Serilog:Properties:Version";

        /// <summary>
        /// A collection of configuration providers for the application to compose. This is useful for adding new configuration sources and providers.
        /// </summary>
        public ConfigurationManager Configuration { get => _builder.Configuration; }

        /// <summary>
        /// A collection of services for the application to compose. This is useful for adding user provided or framework provided services.
        /// </summary>
        public IServiceCollection Services { get => _builder.Services; }

        /// <summary>
        /// An <see cref="IWebHostBuilder"/> for configuring server specific properties, but not building.
        /// To build after configuration, call <see cref="Build"/>.
        /// </summary>
        public ConfigureWebHostBuilder WebHost { get => _builder.WebHost; }

        /// <summary>
        /// An <see cref="IHostBuilder"/> for configuring host specific properties, but not building.
        /// To build after configuration, call <see cref="Build"/>.
        /// </summary>
        public ConfigureHostBuilder Host { get => _builder.Host; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="serviceFriendlyName">This is the name that will display in Event logs. </param>
        /// <param name="logger"></param>
        private AlkamiServiceHost(WebApplicationBuilder builder, string serviceFriendlyName, ILog logger)
        {
            _builder = builder;
            _friendlyName = serviceFriendlyName;
            _logger = logger;
        }

        /// <summary>
        /// Create a <see cref="AlkamiServiceHost"/> with some default configuration
        /// </summary>
        /// <typeparam name="TStartup"></typeparam>
        /// <param name="args">Command line arguments</param>
        /// <returns>The <see cref="AlkamiServiceHost"/> for the application</returns>
        public static AlkamiServiceHost CreateDefaultBuilder<TStartup>(string[] args) where TStartup : class
        {
            return CreateDefaultBuilder<TStartup>(null, args);
        }

        /// <summary>
        /// Create a <see cref="AlkamiServiceHost"/> with some default configuration
        /// </summary>
        /// <typeparam name="TStartup"></typeparam>
        /// <param name="serviceFriendlyName">Friendly name of the application that gets displayed in event logs</param>
        /// <param name="args">Command line arguments</param>
        /// <returns>The <see cref="AlkamiServiceHost"/> for the application</returns>
        public static AlkamiServiceHost CreateDefaultBuilder<TStartup>(string serviceFriendlyName, string[] args) where TStartup : class
        {
            //need to set the AppContext.BaseDirectory here in the options so an error is not thrown when running as a Windows Service
            var options = new WebApplicationOptions
            {
                Args = args,
                ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default
            };

            if (string.IsNullOrWhiteSpace(serviceFriendlyName))
            {
                serviceFriendlyName = typeof(TStartup).Assembly.GetName().Name;
            }

            var _builder = WebApplication.CreateBuilder(options);

            // Setup Logging
            if (ServiceUrlSettings.IsRunningInKubernetes())
            {
                // Setup console logger
                Log.Logger = new LoggerConfiguration().CreateMonitoredConsoleLogger();
                _builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
                _builder.Host.UseSerilog();

                _builder.Configuration.AddJsonFile("config/appsettings.k8.json", optional: true, reloadOnChange: true);
            }
            else
            {
                // Setup file & console logger for local dev. File logger for production
                _builder.Host.ConfigureAppConfiguration((context, builder) =>
                {
                    ConfigureAppConfigurationHandler(context.HostingEnvironment, builder, args, serviceFriendlyName, typeof(TStartup));

                    var serilogConfiguration = builder.Build();
                    LogConfiguration logConfiguration = new();
                    serilogConfiguration.GetSection("LogConfiguration").Bind(logConfiguration);
                    LogManager.Configure(logConfiguration);
                });

                _builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

                _builder.Host.UseSerilog((context, config) =>
                {
                    config.ReadFrom.Configuration(context.Configuration);
                });
            }

            // Windows service information
            _builder.Host.UseWindowsService(options => options.ServiceName = serviceFriendlyName);

            // Setup listeners
            ConfigureHttpListeners(_builder);

            ConfigureServicesHandler(_builder.Services);

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var logger = LogManager.GetLogger<DistributedServiceBase<TStartup>>();
                logger.Fatal(f => f("UNHANDLED APPDOMAIN EXCEPTION!!! SENDER: {0}. MSG: {1}", sender, args.ExceptionObject));
            };
            var logger = LogManager.GetLogger<DistributedServiceBase<TStartup>>();

            var host = new AlkamiServiceHost(_builder, serviceFriendlyName, logger);

            return host;
        }

        internal static void ConfigureServicesHandler(IServiceCollection services)
        {
            services.AddResponseCompression(options => options.EnableForHttps = true);
            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.SmallestSize;
            });

            if (!ServiceUrlSettings.IsRunningInKubernetes())
            {
                services.TryAddSingleton<IInternalServiceResolver, InternalServiceResolver>();
                services.AddHostedService<SubscriptionHostedService>();
            }
        }


        /// <summary>
        /// Builds the <see cref="WebApplication"/>.
        /// </summary>
        /// <returns>A configured <see cref="WebApplication"/>.</returns>
        public WebApplication Build()
        {
            _builder.Services.AddNewRelicMonitor(_builder.Configuration, options =>
            {
                options.AreaName = _friendlyName;
            });

            AlkamiException.Initialize();

            ThreadingConfiguration.UpdateMinThreadsIfOverrideExists();

            var app = _builder.Build();

            app.UseResponseCompression();

            app.Lifetime.ApplicationStopped.Register(() =>
            {
                Broadcaster.StopPublisher();
                Subscription.StopSubscriber();
            });

            var healthCheckMapper = app.Services.GetService<IHealthCheckMapper>() ?? throw new InvalidOperationException("No health check mapper defined. Ensure the service is running UseAlkamiBasicHealthCheck() or UseAlkamiDatabaseHealthCheck() during configuration of the host.");

            if (ServiceUrlSettings.IsRunningInKubernetes())
            {
                // Graceful shutdown behavior
                app.Lifetime.ApplicationStopped.Register(() => RequestLinkerdShutdown());

                // Healthcheck configuration
                healthCheckMapper.MapHealthChecks(app);
            }

            return app;
        }

        private void RequestLinkerdShutdown()
        {
            if (ServiceUrlSettings.IsRunningInKubernetes())
            {
                try
                {
                    _logger.Debug("Sending shutdown signal to Linkerd sidecar");
                    var shutdownResponse = new HttpClient().PostAsync("http://localhost:4191/shutdown", null).Result;
                    _logger.Debug("Linkerd Response:\n" + JsonSerializer.Serialize(shutdownResponse));
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error gracefully shutting down LinkerD: {ex}");
                }
            }
        }

        private static WebApplicationBuilder ConfigureHttpListeners(WebApplicationBuilder builder)
        {
            if (ServiceUrlSettings.IsRunningInKubernetes())
            {
                builder.WebHost.UseKestrel(options => { options.AllowSynchronousIO = true; })
                               .UseUrls($"http://+:{ListenerPortConfiguration.AppPort};http://+:{ListenerPortConfiguration.HealthPort}");
            }
            else
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(Environment.MachineName);

                var ipAddressLocal = hostEntry.AddressList.First();
                var ipAddress = IPAddress.IPv6Any;

                if (ipAddressLocal.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipAddress = IPAddress.Any;
                }

                builder.WebHost
                    .UseNetTcp(ipAddress, 0)
                    .UseKestrel(so =>
                    {
                        so.AllowSynchronousIO = true;
                        so.ListenAnyIP(0);
                    });
            }

            return builder;
        }

        internal static void ConfigureAppConfigurationHandler(IHostEnvironment hostEnvironment, IConfigurationBuilder builder, string[] args, string friendlyName, Type serviceType)
        {
            var assemblyName = serviceType.Assembly.GetName();
            var assemblyVersion = assemblyName.Version ?? new Version(1, 0);
            var assemblyVersionString = assemblyVersion.ToString();
            var psAssembly = typeof(AlkamiServiceHost).Assembly;

            builder
                .AddJsonStream(psAssembly.GetManifestResourceStream(
                    $"{psAssembly.GetName().Name}.Configuration.appsettings.{hostEnvironment.EnvironmentName}.default.json"))
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{hostEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);

            builder.AddInMemoryCollection(new Dictionary<string, string>()
                {
                // Overrides the default appsettings configuration to set the path based on the PackageName.
                { SerilogConfigurationPathKey, @$"C:\OrbLogs\{assemblyName.Name}-.slog" }, });

            builder.AddEnvironmentVariables();

            if (args != null)
            {
                builder.AddCommandLine(args);
            }

            builder
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { PackageName, assemblyName.Name },
                    { FriendlyServiceName, friendlyName },
                    { ServiceVersion, assemblyVersionString },
                    { SerilogPackageNameKey, assemblyName.Name },
                    { SerilogVersionKey, assemblyVersionString },
                });
        }
    }
}
#endif
