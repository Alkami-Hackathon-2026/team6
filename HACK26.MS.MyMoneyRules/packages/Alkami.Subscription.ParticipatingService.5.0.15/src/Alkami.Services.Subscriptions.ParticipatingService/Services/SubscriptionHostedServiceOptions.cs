#if NET6_0_OR_GREATER
using System;
using Alkami.Services.Subscriptions.Data;

namespace Alkami.Services.Subscriptions.ParticipatingService.Services
{
    internal class SubscriptionHostedServiceOptions
    {
        public string Name { get; set; }

        public string FriendlyName { get; set; }

        public Version CurrentVersion { get; set; }

        public string ServicePath { get; set; }

        public ProviderConfiguration ProviderConfiguration { get; set; }

    }
}
#endif
