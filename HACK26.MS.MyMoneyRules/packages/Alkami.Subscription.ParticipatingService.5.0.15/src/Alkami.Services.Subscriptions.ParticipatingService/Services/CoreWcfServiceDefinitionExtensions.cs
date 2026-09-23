#if NET6_0_OR_GREATER
using System;
using System.Runtime.InteropServices;
using Alkami.Contracts;
using Alkami.Services.Subscriptions.Data;

namespace Alkami.Services.Subscriptions.ParticipatingService.Services
{
    /// <summary>
    /// 
    /// </summary>
    public static class CoreWcfServiceDefinitionExtensions
    {

        /// <summary>
        /// Converts a <see cref="CoreWcfServiceDefinition"/> to an <see cref="ServiceDefinition"/>
        /// </summary>
        /// <param name="wcfSettings">The <see cref="CoreWcfServiceDefinition"/> to convert</param>
        /// <param name="port">The port that the service is running on</param>
        /// <param name="metadataPort">The port that the metadata endpoint is running on</param>
        /// <returns></returns>
        public static ServiceDefinition ToServiceDefintion(this CoreWcfServiceDefinition wcfSettings, int port, int metadataPort)
        {
            if (wcfSettings == null)
            {
                return null;
            }
            string schema = "net.tcp";
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                schema = "http";
            }
            var uri = $"{schema}://{Environment.MachineName}:{port}/{wcfSettings.ServicePath}";

            return new ServiceDefinition()
            {
                MetadataPortNumber = metadataPort,
                CurrentVersion = wcfSettings.CurrentVersion,
                Name = wcfSettings.Name,
                FriendlyName = wcfSettings.FriendlyName,
                ProcessId = Environment.ProcessId,
                SupportedClaimsIdentitySerializationMethods = ClaimsIdentitySerializationMethod.DataContractSerializer |
                            ClaimsIdentitySerializationMethod.Json |
                            ClaimsIdentitySerializationMethod.CompressedJson,
                EndpointUri = new Uri(uri),
                ProviderConfiguration = wcfSettings.ProviderConfiguration,
                TenantWhitelistRegex = "(.*?)"
            };
        }

    }
}

#endif
