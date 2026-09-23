using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
#if NET5_0_OR_GREATER
using CoreWCF;
using CoreWCF.Channels;
using CoreWCF.Description;
using CoreWCF.Dispatcher;
#endif
#if NETFRAMEWORK
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
#endif

namespace Alkami.Services.Subscriptions.ParticipatingService
{
    public class RequestResponseBehavior : IServiceBehavior
    {
        private RequestResponseInspector Inspector { get; set; }

        public RequestResponseBehavior(RequestResponseInspector inspector)
        {
            Inspector = inspector;
        }

        public int GetAndResetMessageCount()
        {
            return Inspector.GetAndResetMessageCount();
        }

        public Dictionary<Guid, int> GetAndResetTenantMessageCount()
        {
            return Inspector.GetAndResetTenantCounts().ToDictionary(kvp=>kvp.Key, kvp=>kvp.Value);
        }

        public Timings GetAndResetAverageCallDuration()
        {
            return Inspector.GetAndResetAverageDuration();
        }

        public Dictionary<string, int> GetAndResetErrorsByCode()
        {
            return Inspector.GetAndResetErrorsByCode().ToDictionary(kvp => kvp.Key, kvp => kvp.Value); ;
        }

        public int HighWaterMark
        {
            get { return Inspector.HighWaterMark; }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase,
            Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher channelDispatcher in serviceHostBase.ChannelDispatchers)
            {
                foreach (var endpoint in channelDispatcher.Endpoints)
                {
                    foreach (var dispatchOperation in endpoint.DispatchRuntime.Operations)
                    {
                        dispatchOperation.ParameterInspectors.Add(Inspector);
                    }
                }
            }
        }
    }
}