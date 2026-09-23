#if NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Alkami.Broker.App;
using Alkami.Broker.MessageTemplates;
using Common.Logging;

namespace Alkami.Services.Subscriptions.ParticipatingService
{

	static class ServiceInstanceManager
	{
		private static string _serviceName;
		private static string _machineName;
		private static int _processId;

		private static readonly ILog Logger = LogManager.GetLogger("Alkami.Services.Subscriptions.ParticipatingService.ServiceInstanceManager");


		private const string StopServiceInstanceScript = @"
function now(){{
	[string]$getNow = Get-Date -format G
	return $getNow
}}
$logFilePath = \""C:\\OrbLogs\\Alkami.EagleEye.Manipulation.log\""

 \""$(now) - Received stopping service command for Process {0}.\"" | Out-File $logFilePath -Append
$serviceName = Get-WmiObject -Class Win32_Service -Filter 'ProcessId = {0}' | Select-Object -ExpandProperty Name
if($serviceName -eq $null){{
    \""$(now) - Cannot find service with processId {0}.\"" | Out-File $logFilePath -Append
	exit
}}
try{{
	\""$(now) - Stopping service $serviceName...\"" | Out-File $logFilePath -Append
	stop-service $serviceName -ErrorAction Stop
}}
catch{{
    \""$(now) - Failed to stop Service $serviceName. \"" | Out-File $logFilePath -Append
    Stop-Process {0} -Force
}}";

		private const string StartServiceInstanceScript = @"
$serviceInstance = Get-Service -Name $serviceName
$attempts = 0
$maxAttempts = 5
\""$(now) - Service Status: $($serviceInstance.Status)\"" | Out-File $logFilePath -Append
while($serviceInstance.Status -ne 'Running' -and $attempts -lt $maxAttempts){{
    try{{
        \""$(now) - Starting Service $serviceName...\""  | Out-File $logFilePath -Append
        start-service $serviceName -ErrorAction Stop
        $serviceInstance = Get-Service -Name $serviceName
		\""$(now) - Success! Service Status: $($serviceInstance.Status)\"" | Out-File $logFilePath -Append
    }}
    catch{{
        $attempts = $attempts + 1
		\""$(now) - Failed to start service. Tried $attempts times. Trying again...\"" | Out-File $logFilePath -Append
    }}
}}";
		internal static void Init(string serviceName, string machineName, int processId)
		{
			_serviceName = serviceName;
			_machineName = machineName;
			_processId = processId;
			Subscription.Add(Events.ManipulateServiceInstance, ManipulateServiceInstanceInternal);
		}

		private static void ManipulateServiceInstanceInternal(Dictionary<string, string> dictionary)
		{
			var args = dictionary.ConvertArgsTo<ServiceControllerArgs>();
			if(Logger.IsTraceEnabled)
			{
				Logger.TraceFormat("Received manipulate service instance event for {0} on {1} with action {2}.", args.ServiceName, args.MachineName, args.Action);
			}
			if (args.IsValid() && args.ServiceName.Equals(_serviceName, StringComparison.CurrentCultureIgnoreCase) &&
			    args.MachineName.Equals(_machineName, StringComparison.CurrentCultureIgnoreCase) &&
			    args.ProcessId == _processId)
			{
				if (Logger.IsTraceEnabled)
				{
					Logger.TraceFormat("Args matches service definition. Going to {0} service.", args.Action);
				}
				ManipulateServiceInstanceExternally(args);
			}
			else
			{
				if (Logger.IsTraceEnabled)
				{
					Logger.Trace("Args doesn't match service definition. Skipped.");
				}
			}
		}

		public static void ManipulateServiceInstanceExternally(ServiceControllerArgs args)
		{
			var script = string.Empty;
			if (args.Action.Equals("Stop", StringComparison.CurrentCultureIgnoreCase))
			{
				script = string.Format(StopServiceInstanceScript, args.ProcessId.Value);
			}
			if (args.Action.Equals("Restart", StringComparison.CurrentCultureIgnoreCase))
			{
				script = string.Format(StopServiceInstanceScript + StartServiceInstanceScript, args.ProcessId.Value);
			}
			var process = new Process
			{
				StartInfo =
				{
					FileName = "powershell.exe",
					Arguments = string.Format("-command \"{0}\" ", script)
				}
			};

			process.Start();
		}
	}
}
#endif
