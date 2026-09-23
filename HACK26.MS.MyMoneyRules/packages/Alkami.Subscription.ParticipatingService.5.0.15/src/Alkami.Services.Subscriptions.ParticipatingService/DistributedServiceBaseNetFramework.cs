#if NETFRAMEWORK
using Alkami.Exceptions;
using Alkami.Monitoring;
using Alkami.Services.Subscriptions.Data;
using Alkami.Services.Subscriptions.Resolver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Threading;
using Alkami.Broker.App;
using Alkami.Contracts;
using log4net;
using ILog = Common.Logging.ILog;
using LogManager = Common.Logging.LogManager;
using System.Threading.Tasks;

namespace Alkami.Services.Subscriptions.ParticipatingService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public abstract partial class DistributedServiceBase<T>
    {
        private static readonly ILog Logger = LogManager.GetLogger<DistributedServiceBase<T>>();
        protected readonly ServiceHost Host;
        protected readonly ServiceDefinition _myServiceDefinition;
        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(false);
        private readonly RequestResponseBehavior _behavior;
        private PerformanceCounter _theCpuCounter;
        private PerformanceCounter _theMemCounter;
        private readonly string _processName;
        protected readonly CancellationTokenSource _serviceCancellationToken;
        private RegisteredWaitHandle registeredWaitHandle;
        private const string Debugpidsettingname = "DebugPID";
        private const string RunAsConsumerOnlyName = "DebugRunAsConsumerOnly";
        private readonly bool RunAsConsumerOnly;
        protected string EndpointAddress;
        protected Binding Binding;

        protected DistributedServiceBase(string friendlyName = null) : this(friendlyName, null) { }

        protected DistributedServiceBase(string friendlyName, string fullQualifiedName)
        {
            var qualifiedName = fullQualifiedName ?? typeof(T).FullName;

            friendlyName = friendlyName ?? typeof(T).Name;

            try
            {
                AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                {
                    Logger.Fatal(f => f("UNHANDLED APPDOMAIN EXCEPTION!!! SENDER: {0}. MSG: {1}", sender, args.ExceptionObject));
                };

                var startTime = DateTimeOffset.Now;
                var process = Process.GetCurrentProcess();
                _processName = process.ProcessName;

                _serviceCancellationToken = new CancellationTokenSource();

                var pidExtention = process.Id;

                if (ConfigurationManager.AppSettings[Debugpidsettingname] != null)
                {
                    int.TryParse(ConfigurationManager.AppSettings[Debugpidsettingname], out pidExtention);
                }

                if (ConfigurationManager.AppSettings[RunAsConsumerOnlyName] != null)
                {
                    bool.TryParse(ConfigurationManager.AppSettings[RunAsConsumerOnlyName], out RunAsConsumerOnly);
                }

                var serviceUrl = string.Format("net.tcp://{0}:12016/{1}", Environment.MachineName, pidExtention);

                Host = new ServiceHost(this, new Uri(serviceUrl));

                Binding = SubscriptionBindings.NetTcpBinding();

                EndpointAddress = string.Format("{0}/{1}", serviceUrl, qualifiedName);

                var endpoint = Host.AddServiceEndpoint
                (
                    qualifiedName,
                    Binding,
                    EndpointAddress
                );


                _behavior = new RequestResponseBehavior(new RequestResponseInspector());

                Host.Description.Behaviors.Add(_behavior);

                foreach (var descriptionEndpoint in Host.Description.Endpoints)
                {
                    descriptionEndpoint.Behaviors.Add(new SoapLoggingEndpointBehavior());
                }

                var throttle = Host.Description.Behaviors.Find<ServiceThrottlingBehavior>();

                if (throttle == null)
                {
                    throttle = new ServiceThrottlingBehavior();
                    Host.Description.Behaviors.Add(throttle);
                }

                throttle.MaxConcurrentCalls = int.MaxValue;
                throttle.MaxConcurrentInstances = int.MaxValue;
                throttle.MaxConcurrentSessions = int.MaxValue;

                foreach (var operation in endpoint.Contract.Operations)
                {
                    operation.Behaviors.Add(new GenericErrorHandler());
                    var serializer = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
                    if (serializer != null)
                    {
                        serializer.MaxItemsInObjectGraph = int.MaxValue;
                    }
                }

                Host.Credentials.ClientCertificate.Certificate = SubscriptionBindings.ClientCertificate;
                Host.Credentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.Offline;
                Host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;
                Host.Credentials.ServiceCertificate.Certificate = SubscriptionBindings.ServiceCertificate;

                var whiteList = ConfigurationManager.AppSettings["TenantWhitelistRegex"];
                var currentAssemblyVersion = Helpers.GetCurrentAssemblyVersion();
                GlobalContext.Properties["AssemblyVersion"] = currentAssemblyVersion;

                _myServiceDefinition = new ServiceDefinition()
                {
                    CurrentVersion = currentAssemblyVersion,
                    EndpointUri = endpoint.ListenUri,
                    Name = qualifiedName,
                    FriendlyName = friendlyName,
                    ProcessId = pidExtention,
                    SupportedClaimsIdentitySerializationMethods =
                        (ClaimsIdentitySerializationMethod.DataContractSerializer
                        | ClaimsIdentitySerializationMethod.Json
                        | ClaimsIdentitySerializationMethod.CompressedJson),
                };

                if (String.IsNullOrWhiteSpace(whiteList))
                {
                    whiteList = "(.*?)";
                }
                _myServiceDefinition.TenantWhitelistRegex = whiteList;

                var meta = new Uri(_myServiceDefinition.ToHttpServiceUrl());

                Console.WriteLine("Meta-data @ {0}", meta);
                Logger.InfoFormat("Meta-data @ {0}", meta);

                var smb = new ServiceMetadataBehavior
                {
                    HttpGetEnabled = true,
                    HttpGetUrl = meta
                };
                Host.Description.Behaviors.Add(smb);

                Metric.AreaName = friendlyName;

                AlkamiException.Initialize();
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Unable to startup service [{0}] due to error:", e, qualifiedName);
                throw;
            }
        }

        private void HandleServiceStateChange(Dictionary<string, string> statusChange)
        {
            if (this._serviceCancellationToken.IsCancellationRequested)
                return;

            ServiceResolver.Refresh();
        }

        public Task StartAsync()
        {
            Start();
            return Task.CompletedTask;
        }

        public void Start()
        {
            Subscription.Add(Events.StateChangeArgs, HandleServiceStateChange);
            _theCpuCounter = new PerformanceCounter("Process", "% Processor Time", _processName);
            _theMemCounter = new PerformanceCounter("Process", "Working Set - Private", _processName);

            Host.Open();

            Console.WriteLine();
            Console.Write("Connecting to Subscription Service .");

            while (!ServiceResolver.EstablishedConnectionAtLeastOnce)
            {
                Console.Write(".");
                Thread.Sleep(1000);

                ServiceResolver.ForceRefresh();
            }

            Console.WriteLine(" connected!");

            Console.WriteLine("Registering service with subscription service ...");

            if (!RunAsConsumerOnly)
            {
                ServiceResolver.Register(_myServiceDefinition);
                Console.WriteLine("Successfully registered with subscription service. To show service metadata and an active list of registered services, please enable trace logging (for either the console or file logger) on the Alkami.Services.Subscriptions.Resolver.InnerResolver namespace.");
            }

            registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(_resetEvent, (a, b) =>
            {
                try
                {
                    _myServiceDefinition.ErrorsByCode = _behavior.GetAndResetErrorsByCode();
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("Unable to update performance metrics in service definition. Error message: {0}", e.Message);
                }

                if (!RunAsConsumerOnly && ServiceResolver.EstablishedConnectionAtLeastOnce)
                {
                    ServiceResolver.Register(_myServiceDefinition);
                    Console.WriteLine("Successfully registered with subscription service. To show service metadata and an active list of registered services, please enable trace logging (for either the console or file logger) on the Alkami.Services.Subscriptions.Resolver.InnerResolver namespace.");
                }
            },
            null,
            TimeSpan.FromMinutes(1),
            false);

            ServiceInstanceManager.Init(_myServiceDefinition.Name, Environment.MachineName, _myServiceDefinition.ProcessId);
        }

        public Task StopAsync(TimeSpan timeout)
        {
            Stop(timeout);
            return Task.CompletedTask;
        }

        public void Stop(TimeSpan timeout)
        {

            Logger.Info("Preparing to stop distributed service....");
            if (_theCpuCounter != null)
                _theCpuCounter.Dispose();
            if (_theMemCounter != null)
                _theMemCounter.Dispose();

            _serviceCancellationToken.Cancel();

            if (registeredWaitHandle != null)
                registeredWaitHandle.Unregister(_resetEvent);

            if (!RunAsConsumerOnly)
                ServiceResolver.UnRegister(_myServiceDefinition);

            Broadcaster.StopPublisher();
            Subscription.StopSubscriber();
            Host.Close(timeout);
        }
    }
}
#endif
