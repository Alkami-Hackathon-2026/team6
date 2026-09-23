using Alkami.Contracts;
using Alkami.Services.Subscriptions.ParticipatingClient;

namespace Alkami.MicroServices.Settings.ProviderBasedClient
{
    internal class ProviderServiceData<T> where T : class
    {
        public ClaimsIdentitySerializationMethod ClaimsIdentitySerializationMethod { get; set; }

        public AlkamiCachedClient<T> Client { get; set; }
    }
}