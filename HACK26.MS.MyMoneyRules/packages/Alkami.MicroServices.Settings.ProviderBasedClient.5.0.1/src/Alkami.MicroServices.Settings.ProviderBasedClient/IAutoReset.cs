using System;

namespace Alkami.MicroServices.Settings.ProviderBasedClient
{
    public interface IAutoReset : IDisposable
    {
        event EventHandler TimeoutEvent;
        int TimeoutSeconds { get; }

        void SetResetTimeoutSeconds(int Seconds);
        void Start();
        void Stop();
        void ForceRaiseTimeoutEvent();
    }
}
