using Alkami.TrackableObjects.Plugins;
using HACK26.MS.MyMoneyRules.Service.Integrations;
using System;

namespace HACK26.MS.MyMoneyRules.Service
{
    /// <inheritdoc />
    public partial class ServiceImp : Plugin
    {
        /// <summary>
        /// Shared default client so every ServiceImp instance reuses one HTTP pipeline
        /// </summary>
        private static readonly Lazy<IGeminiClient> DefaultGeminiClient = new Lazy<IGeminiClient>(() => new GeminiClient());

        private readonly IGeminiClient _geminiClient;
        private string _providerName = "";

        /// <summary>
        /// Create a new service for this object
        /// </summary>
        /// <param name="providerType"></param>
        public ServiceImp(string providerType) : base(providerType)
        {
            _providerName = GetType().AssemblyQualifiedName;
            _geminiClient = DefaultGeminiClient.Value;
        }

        /// <summary>
        /// Create a new service for this object
        /// </summary>
        /// <param name="providerType"></param>
        /// <param name="providerName"></param>
        public ServiceImp(string providerType, string providerName) : this(providerType, providerName, null)
        {
        }

        /// <summary>
        /// Create a new service for this object with explicit integrations
        /// </summary>
        /// <param name="providerType"></param>
        /// <param name="providerName"></param>
        /// <param name="geminiClient">Gemini client; the shared default is used when null</param>
        public ServiceImp(string providerType, string providerName, IGeminiClient geminiClient) : base(providerType, providerName)
        {
            _providerName = string.IsNullOrWhiteSpace(providerName) ? GetType().AssemblyQualifiedName : providerName;
            _geminiClient = geminiClient ?? DefaultGeminiClient.Value;
        }

        /// <summary>
        /// Don't change this on configurable microservices.
        /// </summary>
        public override string ItemType => "Connector";

        /// <summary>
        /// Defines the name of this microservice
        /// </summary>
        public override string Name => _providerName;

        /// <summary>
        /// This is a friendly descriptive name of the service these configurations are a part of
        /// </summary>
        public const string StaticName = "HACK26 MyMoneyRules";

        /// <summary>
        /// Our provider types can be customized. Any sample provider created by Alkami's SDK team will have the "SDKSample" provider type.
        /// </summary>
        public const string StaticProviderType = "HACK26";

        /// <summary>
        /// This is the unique name of the provider. This cannot be shared with other providers and is usually the base namespace of this solution.
        /// </summary>
        public const string StaticProviderName = "HACK26.MS.MyMoneyRules";
    }
}
