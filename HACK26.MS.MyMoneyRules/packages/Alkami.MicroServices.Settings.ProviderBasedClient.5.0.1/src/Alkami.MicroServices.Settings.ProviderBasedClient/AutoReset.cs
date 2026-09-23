using Alkami.Utilities.Configuration;
using System;
using System.Threading;

namespace Alkami.MicroServices.Settings.ProviderBasedClient
{
    public class AutoReset : IAutoReset
    {
        public event EventHandler TimeoutEvent;
        public int TimeoutSeconds { get { return _refreshIntervalSeconds; } }

        private string _timeoutSettingName;
        private int _minimumRefreshIntervalSeconds;
        private readonly object _eventResetHandlerInitializeLock = new object();
        private bool _autoResetHasBeenInitialized;
        private int _refreshIntervalSeconds;
        private AutoResetEvent _autoResetEvent;
        private RegisteredWaitHandle _handle;

        public AutoReset(int minimumIntervalTimeout, string intervalTimeoutAppSettingName = null)
        {
            _minimumRefreshIntervalSeconds = minimumIntervalTimeout;
            _timeoutSettingName = intervalTimeoutAppSettingName;
        }

        public void SetResetTimeoutSeconds(int seconds)
        {
            if (seconds >= 1 && (seconds != _refreshIntervalSeconds || _refreshIntervalSeconds == 0))
            {
                _refreshIntervalSeconds = seconds;

                _autoResetHasBeenInitialized = false;
            }
        }

        public void Start()
        {
            if (_autoResetHasBeenInitialized)
            {
                return;
            }

            lock (_eventResetHandlerInitializeLock)
            {
                if (_autoResetHasBeenInitialized)
                {
                    return;
                }

                if (_refreshIntervalSeconds <= 0)
                {
                    if (!string.IsNullOrWhiteSpace(_timeoutSettingName))
                    {
                        var settingValue = Manager.GetSetting(_timeoutSettingName);

                        int.TryParse(settingValue, out _refreshIntervalSeconds);
                    }

                    if (_refreshIntervalSeconds < _minimumRefreshIntervalSeconds)
                    {
                        _refreshIntervalSeconds = _minimumRefreshIntervalSeconds;
                    }
                }

                SetWaitHandle();

                _autoResetHasBeenInitialized = true;
            }         
        }

        public void Stop()
        {
            Dispose();
        }

        public void Dispose()
        {
            _autoResetHasBeenInitialized = false;

            if (_handle != null)
            {
                if (_autoResetEvent != null)
                {
                    _handle.Unregister(_autoResetEvent);
                }

                _handle = null;
            }

            if (_autoResetEvent != null)
            {
                _autoResetEvent.Dispose();
                _autoResetEvent = null;
            }
        }

        public void ForceRaiseTimeoutEvent()
        {
            RaiseTimeoutEvent();
        }

        private void SetWaitHandle()
        {
            Dispose();

            _autoResetEvent = new AutoResetEvent(false);

            _handle = ThreadPool.RegisterWaitForSingleObject(
                _autoResetEvent,
                new WaitOrTimerCallback(HandleAutoReset),
                null,
                TimeSpan.FromSeconds(_refreshIntervalSeconds),
                false);
        }

        private void HandleAutoReset(object state, bool timedOut)
        {
            RaiseTimeoutEvent();
        }

        private void RaiseTimeoutEvent()
        {
            if (TimeoutEvent != null)
            {
                TimeoutEvent(this, null);
            }
        }
    }
}
