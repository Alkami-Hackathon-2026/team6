#if NET6_0_OR_GREATER
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Alkami.Services.Subscriptions.Data;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CoreWCF.Description;

namespace Alkami.Services.Subscriptions.ParticipatingService.Services
{
    internal class SubscriptionHostedService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly TaskCompletionSource _source = new();

        private readonly IInternalServiceResolver ServiceResolver;
        internal readonly List<ServiceDefinition> _serviceDefinitions;
        private readonly List<CoreWcfServiceDefinition> Options;
        private readonly ILogger<SubscriptionHostedService> Logger;

        internal TimeSpan RegisterInterval = TimeSpan.FromMinutes(1);
        bool wasRegistered = false;

        public SubscriptionHostedService(IServiceProvider services, IHostApplicationLifetime lifetime, IInternalServiceResolver serviceResolver, IEnumerable<CoreWcfServiceDefinition> options, ILogger<SubscriptionHostedService> logger)
        {
            _serviceDefinitions = new List<ServiceDefinition>();

            ServiceResolver = serviceResolver;
            Options = options.ToList();
            Logger = logger;

            _services = services;
            _lifetime = lifetime;
            _lifetime.ApplicationStarted.Register(() => _source.SetResult());
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //Based on AndrewLock setup: https://andrewlock.net/finding-the-urls-of-an-aspnetcore-app-from-a-hosted-service-in-dotnet-6/
            // 👇 Create a TaskCompletionSource for the stoppingToken
            var tcs = new TaskCompletionSource();
            stoppingToken.Register(() => tcs.SetResult());

            // wait for _either_ of the sources to complete
            await Task.WhenAny(tcs.Task, _source.Task).ConfigureAwait(false);

            // if cancellation was requested, stop 
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            var ports = SetupServiceDefinitionAndReturnPorts();

            var requestHeadersBehavior = _services.GetServices<IServiceBehavior>().FirstOrDefault(x => x is UseRequestHeadersForMetadataAddressBehavior) as UseRequestHeadersForMetadataAddressBehavior;

            if (requestHeadersBehavior != null)
            {
                requestHeadersBehavior.DefaultPortsByScheme["net.tcp"] = ports.netTcpPort;
            }

            Console.WriteLine();
            Console.Write("Connecting to Subscription Service .");

            while (!ServiceResolver.EstablishedConnectionAtLeastOnce && !stoppingToken.IsCancellationRequested)
            {
                Logger.LogDebug("Trying to connect to Subscription Service");
                Console.Write(".");

                await Task.Delay(1000, stoppingToken);
                await ServiceResolver.ForceRefresh();
            }

            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            Logger.LogDebug("Connected to Subscription Service");
            Console.WriteLine(" connected!");

            Console.WriteLine("Registering service with subscription service ...");
            foreach (var serviceDefintion in _serviceDefinitions)
            {
                await ServiceResolver.Register(serviceDefintion);
            }
            Console.WriteLine("Registered!");

            wasRegistered = true;

            Logger.LogDebug("Registered with Subscription Service");

            using var timer = new PeriodicTimer(RegisterInterval);
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                if (ServiceResolver.EstablishedConnectionAtLeastOnce)
                {
                    foreach (var serviceDefintion in _serviceDefinitions)
                    {
                        await ServiceResolver.Register(serviceDefintion);
                    }
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (wasRegistered)
            {
                foreach (var serviceDefintion in _serviceDefinitions)
                {
                    await ServiceResolver.Unregister(serviceDefintion);
                }
            }

            await base.StopAsync(cancellationToken);
        }


        internal (int netTcpPort, int metadataPort) SetupServiceDefinitionAndReturnPorts()
        {
            var server = _services.GetRequiredService<IServer>();
            var serverAddressesFeature = server.Features.Get<IServerAddressesFeature>();
            var listeningAddresses = serverAddressesFeature.Addresses.ToList();

            var netTcpPort = GetPort(listeningAddresses[0]);
            var metadataPort = listeningAddresses.Count > 1 ? GetPort(listeningAddresses[1]) : GetPort(listeningAddresses[0]);

            foreach (var setting in Options)
            {
                _serviceDefinitions.Add(setting.ToServiceDefintion(netTcpPort, metadataPort));
            }
            return (netTcpPort, metadataPort);
        }

        private int GetPort(string address)
        {
            // Parse the address to get the port
            if (Uri.TryCreate(address, UriKind.Absolute, out var uri))
            {
                return uri.Port;
            }
            else
            {
                throw new ArgumentException($"Uri '{address}' cannot be parsed.", nameof(address));
            }
        }
    }
}
#endif
