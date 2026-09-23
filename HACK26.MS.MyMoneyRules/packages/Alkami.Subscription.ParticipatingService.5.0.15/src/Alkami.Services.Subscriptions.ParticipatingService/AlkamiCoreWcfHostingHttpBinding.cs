#if NET6_0_OR_GREATER
using CoreWCF;
using CoreWCF.Channels;

namespace Alkami.Services.Subscriptions.ParticipatingService
{
    public class AlkamiCoreWcfHostingHttpBinding : CustomBinding
    {
        public AlkamiCoreWcfHostingHttpBinding(BasicHttpBinding binding) : base(binding)
        {
            var transportElement = Elements.Find<HttpTransportBindingElement>();
            transportElement.KeepAliveEnabled = false;
        }
    }
}
#endif
