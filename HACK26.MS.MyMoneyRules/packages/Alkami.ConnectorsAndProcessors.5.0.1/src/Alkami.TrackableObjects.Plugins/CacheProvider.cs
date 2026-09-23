namespace Alkami.TrackableObjects.Plugins
{
	public abstract class CacheProvider : Plugin
	{
		protected CacheProvider(string providerType)
            : base(providerType)
        {
		}

		protected CacheProvider(string providerType, string providerName)
            : base(providerType,providerName)
        {
		}

		public override string ItemType
		{
			get { return "CacheProvider"; }
		}
	}
}
