using System;
using System.Collections.Concurrent;

namespace Alkami.MicroServices.Settings.ProviderBasedClient
{
    internal class ProviderCacheContainer
    {
        public DateTime LastRefreshedDateTime = DateTime.Now;
        public ConcurrentDictionary<long, ProviderDefinition> Providers 
            = new ConcurrentDictionary<long, ProviderDefinition>();
    }
}
