namespace Alkami.TrackableObjects.Plugins
{
    public abstract class ConnectorBase : MultiInstancePlugin
    {
        protected ConnectorBase(string providerType)
            : base(providerType)
        {
        }

        protected ConnectorBase(string providerType,string providerName)
            : base(providerType,providerName)
        {
        }

        public override string ItemType
        {
            get { return "Connector"; }
        }
    }
}