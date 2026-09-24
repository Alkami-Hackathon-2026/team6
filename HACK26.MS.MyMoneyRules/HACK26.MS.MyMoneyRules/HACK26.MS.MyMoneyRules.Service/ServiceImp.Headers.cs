using Alkami.TrackableObjects.Plugins;

namespace HACK26.MS.MyMoneyRules.Service
{
    /// <inheritdoc />
    public partial class ServiceImp : Plugin
    {
        private string _providerName = "";

        /// <summary>
        /// Create a new service for this object
        /// </summary>
        /// <param name="providerType"></param>
        public ServiceImp(string providerType) : base(providerType)
        {
            _providerName = GetType().AssemblyQualifiedName;
        }

        /// <summary>
        /// Create a new service for this object
        /// </summary>
        /// <param name="providerType"></param>
        /// <param name="providerName"></param>
        public ServiceImp(string providerType, string providerName) : base(providerType, providerName)
        {
            _providerName = string.IsNullOrWhiteSpace(providerName) ? GetType().AssemblyQualifiedName : providerName;
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
