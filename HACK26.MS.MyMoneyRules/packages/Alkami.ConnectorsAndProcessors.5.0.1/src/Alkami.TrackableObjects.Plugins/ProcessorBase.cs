namespace Alkami.TrackableObjects.Plugins
{
    public abstract class ProcessorBase : MultiInstancePlugin
    {
        protected ProcessorBase(string providerType)
            : base(providerType)
        {
        }

        protected ProcessorBase(string providerType,string providerName)
            : base(providerType,providerName)
        {
        }

        public override string ItemType
        {
            get { return "Processor"; }
        }
    }
}