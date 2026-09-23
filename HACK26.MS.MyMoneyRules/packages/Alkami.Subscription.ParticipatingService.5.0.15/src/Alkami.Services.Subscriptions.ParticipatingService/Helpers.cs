using System;
using System.Reflection;

namespace Alkami.Services.Subscriptions.ParticipatingService
{
    public static class Helpers
    {
        public static Version GetCurrentAssemblyVersion()
        {
            return Assembly.GetEntryAssembly() != null ? Assembly.GetEntryAssembly().GetName().Version : Assembly.GetCallingAssembly().GetName().Version;
        }
    }
}
