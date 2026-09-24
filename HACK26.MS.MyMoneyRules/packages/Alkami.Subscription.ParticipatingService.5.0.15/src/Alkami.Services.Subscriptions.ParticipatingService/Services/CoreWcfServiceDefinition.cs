#if NET6_0_OR_GREATER
using System;
using System.Runtime.Serialization;
using Alkami.Services.Subscriptions.Data;
using CoreWCF.Description;

namespace Alkami.Services.Subscriptions.ParticipatingService.Services
{
    /// <summary>
    /// Settings that are needed in order to configure CoreWCF endpoints and service registration
    /// </summary>
    public class CoreWcfServiceDefinition
    {
        /// <summary>
        /// The <see cref="Type"/> of the Contract
        /// </summary>
        public Type ContractType { get; set; }

        /// <summary>
        /// The <see cref="Type"/> of the Service Implementation
        /// </summary>
        public Type ServiceType { get; set; }

        /// <summary>
        /// The path used in the endpoint for registration and listening
        /// </summary>
        public string ServicePath { get; set; }

        /// <summary>
        /// The name of the service. Should be Contract Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Friendly display name of the service
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Version of the Contract
        /// </summary>
        public Version CurrentVersion { get; set; }

        /// <summary>
        /// If service is a provider this is the provider information
        /// </summary>
        public ProviderConfiguration ProviderConfiguration { get; set; }

        /// <summary>
        /// This is the serialization surrogate provider to help with data contract serialization
        /// </summary>
        public ISerializationSurrogateProvider SerializationSurrogateProvider { get; init; }

        /// <summary>
        /// Adds option to be able to configure the ServiceEndpoint
        /// </summary>
        public Action<ServiceEndpoint> ConfigureServiceEndpoint { get; set; }
    }
}
#endif
