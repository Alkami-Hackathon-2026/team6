#if NET6_0_OR_GREATER
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Alkami.Services.Subscriptions.Data;
using Alkami.Services.Subscriptions.ParticipatingService;
using Alkami.Services.Subscriptions.ParticipatingService.Services;
using Alkami.Utilities.Kubernetes;
using Alkami.Utilities.Rpc;
using CoreWCF.Configuration;
using CoreWCF.Description;
using CoreWCF.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Alkami.AspNetCore.Builder
{
    /// <summary>
    /// 
    /// </summary>
    public static class ParticipatingServiceApplicationBuilderExtensions
    {

        /// <summary>
        /// Registers a WCF service of implementation <typeparamref name="TService"/> with contract type of <typeparamref name="TContract"/>
        /// </summary>
        /// <typeparam name="TContract"></typeparam>
        /// <typeparam name="TService"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddCoreWcfService<TContract, TService>(this IServiceCollection services) where TService : class
        {
            return AddCoreWcfService<TContract, TService>(services, null, null);
        }

        /// <summary>
        /// Registers a WCF service of implementation <typeparamref name="TService"/> with contract type of <typeparamref name="TContract"/>
        /// </summary>
        /// <typeparam name="TContract"></typeparam>
        /// <typeparam name="TService"></typeparam>
        /// <param name="services"></param>
        /// <param name="configureServiceEndpoint">Action to setup custom configuration on an ServiceEndpoint</param>
        /// <returns></returns>
        public static IServiceCollection AddCoreWcfService<TContract, TService>(this IServiceCollection services, Action<ServiceEndpoint> configureServiceEndpoint) where TService : class
        {
            return AddCoreWcfService<TContract, TService>(services, null, configureServiceEndpoint);
        }

        /// <summary>
        /// Registers a WCF service of implementation <typeparamref name="TService"/> with contract type of <typeparamref name="TContract"/>
        /// </summary>
        /// <typeparam name="TContract"></typeparam>
        /// <typeparam name="TService"></typeparam>
        /// <param name="services"></param>
        /// <param name="friendlyName">Friendly Name that shows up in subscription service</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Throws this error if you have already configured a WCF service with contract of <typeparamref name="TContract"/> previously</exception>
        public static IServiceCollection AddCoreWcfService<TContract, TService>(this IServiceCollection services, string friendlyName) where TService : class
        {
            return AddCoreWcfService<TContract, TService>(services, friendlyName, null);
        }

        /// <summary>
        /// Registers a WCF service of implementation <typeparamref name="TService"/> with contract type of <typeparamref name="TContract"/>
        /// </summary>
        /// <typeparam name="TContract"></typeparam>
        /// <typeparam name="TService"></typeparam>
        /// <param name="services"></param>
        /// <param name="friendlyName">Friendly Name that shows up in subscription service</param>
        /// <param name="configureServiceEndpoint">Action to setup custom configuration on an ServiceEndpoint.</param>
        /// <returns></returns>
        public static IServiceCollection AddCoreWcfService<TContract, TService>(this IServiceCollection services, string friendlyName, Action<ServiceEndpoint> configureServiceEndpoint) where TService : class
        {
            if (services.Where(x => x.ServiceType == typeof(CoreWcfServiceDefinition))
                .Select(x => x.ImplementationInstance)
                .Cast<CoreWcfServiceDefinition>()
                .Any(x => x.ContractType == typeof(TContract))
                )
            {
                throw new ArgumentException("Already have registered the Contract");
            }

            var coreWcfSettings = new CoreWcfServiceDefinition
            {
                ContractType = typeof(TContract),
                ServiceType = typeof(TService),
                ServicePath = typeof(TContract).Name,
                CurrentVersion = typeof(TService).Assembly.GetName().Version,
                FriendlyName = friendlyName ?? typeof(TContract).Name,
                Name = typeof(TContract).FullName,
                SerializationSurrogateProvider = SerializationSurrogateProvider.GetDefault(),
                ConfigureServiceEndpoint = configureServiceEndpoint
            };

            services.AddSingleton(coreWcfSettings);

            services.AddCoreWcfServices();

            return services;
        }

        /// <summary>
        /// Setups CoreWcf for the contract
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddCoreWcfServices(this IServiceCollection services)
        {
            if (!services.Any(x => x.ServiceType == typeof(IServiceBuilder)))
            {
                // CoreWCF registrations
                services
                    .AddServiceModelServices()
                    .AddServiceModelMetadata();

                services.TryAddEnumerable(ServiceDescriptor.Singleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>());

                services.TryAddEnumerable(ServiceDescriptor.Singleton<IServiceBehavior>(new RequestResponseBehavior(new RequestResponseInspector())));
            }

            return services;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseCoreWcf(this IApplicationBuilder app)
        {
            // Healthcheck configuration
            var settings = app.ApplicationServices.GetServices<CoreWcfServiceDefinition>().ToList();

            // CoreWCF Binding setup
            app.UseServiceModel(serviceBuilder =>
            {
                if (ServiceUrlSettings.IsRunningInKubernetes())
                {
                    foreach (var service in settings)
                    {
                        AddServiceToBuilderForK8s(serviceBuilder, service);
                    }
                }
                else
                {
                    foreach (var service in settings)
                    {
                        AddServiceToBuilder(serviceBuilder, service);
                    }
                }
            });

            var serviceMetadataBehavior = app.ApplicationServices.GetRequiredService<ServiceMetadataBehavior>();
            serviceMetadataBehavior.HttpGetEnabled = true;
            serviceMetadataBehavior.HttpsGetEnabled = true;

            if (!ServiceUrlSettings.IsRunningInKubernetes())
            {
                var service = settings.FirstOrDefault();
                if (service != null)
                {
                    serviceMetadataBehavior.HttpGetUrl = new Uri($"http://[::]/{service.ServicePath}");
                }
            }
            else
            {
                var service = settings.FirstOrDefault();
                if (service != null)
                {
                    serviceMetadataBehavior.HttpGetUrl = new Uri($"http://[::]/{service.ServicePath}");
                    serviceMetadataBehavior.HttpsGetUrl = new Uri($"https://[::]/{service.ServicePath}");
                }
            }

            return app;
        }

        private static void AddServiceToBuilderForK8s(IServiceBuilder serviceBuilder, CoreWcfServiceDefinition settings)
        {
            serviceBuilder.AddService(settings.ServiceType, options =>
            {
                options.BaseAddresses.Add(new Uri("http://0.0.0.0"));
                options.BaseAddresses.Add(new Uri("https://0.0.0.0"));
            });

            // HTTP Binding - FOR k8s -> K8s communication
            serviceBuilder.AddServiceEndpoint(settings.ServiceType, settings.ContractType, CoreWcfHostingBindings.HttpBinding(https: false), new Uri($"/{settings.ServicePath}", UriKind.RelativeOrAbsolute), (Uri)null, x =>
            {
                SetupOperations(x, settings, false);
            });

            //// HTTPS BINDING - FOR nginx PROXY
            serviceBuilder.AddServiceEndpoint(settings.ServiceType, settings.ContractType, CoreWcfHostingBindings.HttpBinding(https: true), new Uri($"/{settings.ServicePath}", UriKind.RelativeOrAbsolute), (Uri)null, x =>
            {
                SetupOperations(x, settings, false);
            });
        }

        private static void AddServiceToBuilder(IServiceBuilder serviceBuilder, CoreWcfServiceDefinition settings)
        {
            serviceBuilder.AddService(settings.ServiceType);

            bool isInWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (isInWindows)
            {
                // NetTcp Binding - Outside Kubernetes
                serviceBuilder.AddServiceEndpoint(settings.ServiceType, settings.ContractType, CoreWcfHostingBindings.NetTcpBinding(), new Uri($"/{settings.ServicePath}", UriKind.RelativeOrAbsolute), (Uri)null, x =>
                {
                    SetupOperations(x, settings, true);
                });

                serviceBuilder.ConfigureServiceHostBase(settings.ServiceType, host =>
                {
                    host.Credentials.ClientCertificate.Certificate = SubscriptionBindings.ClientCertificate;
                    host.Credentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.Offline;
                    host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerTrust;
                    host.Credentials.ServiceCertificate.Certificate = SubscriptionBindings.ServiceCertificate;
                });
            }
            else
            {
                // NetTcp Binding - Outside Kubernetes
                serviceBuilder.AddServiceEndpoint(settings.ServiceType, settings.ContractType, CoreWcfHostingBindings.HttpBinding(https: false), new Uri($"/{settings.ServicePath}", UriKind.RelativeOrAbsolute), (Uri)null, x =>
                {
                    SetupOperations(x, settings, true);
                });
            }
        }

        private static void SetupOperations(ServiceEndpoint serviceEndpoint, CoreWcfServiceDefinition settings, bool includeNewRelicTransaction)
        {
            foreach (var operation in serviceEndpoint.Contract.Operations)
            {
                if (operation.OperationBehaviors.TryGetValue(typeof(DataContractSerializerOperationBehavior), out var serializer))
                {
                    var dcSerializer = ((DataContractSerializerOperationBehavior)serializer);
                    operation.OperationBehaviors.Remove(typeof(DataContractSerializerOperationBehavior));

                    var alkSerializer = new AlkamiDataContractSerializerOperationBehavior(operation, dcSerializer.DataContractFormatAttribute)
                    {
                        SerializationSurrogateProvider = settings.SerializationSurrogateProvider,
                        MaxItemsInObjectGraph = int.MaxValue
                    };
                    operation.OperationBehaviors.Add(alkSerializer);
                }

                if (!operation.OperationBehaviors.TryGetValue(typeof(GenericErrorHandler), out var _))
                {
                    operation.OperationBehaviors.Add(new GenericErrorHandler());
                }
            }

            settings.ConfigureServiceEndpoint?.Invoke(serviceEndpoint);

            if (includeNewRelicTransaction)
            {
                foreach (var operation in serviceEndpoint.Contract.Operations)
                {
                    if (!operation.OperationBehaviors.TryGetValue(typeof(NewRelicOperationBehavior), out var _))
                    {
                        operation.OperationBehaviors.Add(new NewRelicOperationBehavior());
                    }
                }
            }
        }

    }

}
#endif
