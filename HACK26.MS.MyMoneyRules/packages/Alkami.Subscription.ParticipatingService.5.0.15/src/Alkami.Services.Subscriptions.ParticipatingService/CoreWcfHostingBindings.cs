#if NET6_0_OR_GREATER
using Alkami.Utilities.Cryptography;
using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using CoreWCF;
using CoreWCF.Channels;
using System.Xml;
using Alkami.Utilities.Kubernetes;

namespace Alkami.Services.Subscriptions.ParticipatingService
{
    /// <summary>
    /// Standard Bindings and Helpers for WCF clients and Services
    /// </summary>
    public static class CoreWcfHostingBindings
    {
        /// <summary>
        /// 1 minute timeout
        /// </summary>
        private const int DefaultTimeout = 60;

        /// <summary>
        /// Determines if the current process is running in Kubernetes
        /// </summary>
        /// <returns></returns>
        private static bool IsRunningInKubernetes()
        {
            return ServiceUrlSettings.IsRunningInKubernetes();
        }

        /// <summary>
        /// Standard binding for services and clients
        /// </summary>
        /// <returns></returns>
        public static NetTcpBinding NetTcpBinding()
        {
            return NetTcpBinding(DefaultTimeout);
        }

        /// <summary>
        /// Standard binding for services and clients
        /// </summary>
        /// <param name="timeoutSeconds">The receive and send timeout in seconds. A negative value will be replaced with a default.</param>
        /// <returns></returns>
        public static NetTcpBinding NetTcpBinding(int timeoutSeconds)
        {
            if (timeoutSeconds < 0)
                throw new ArgumentException(nameof(timeoutSeconds) + " cannot be < 0");

            if (IsRunningInKubernetes())
            {
                throw new NotSupportedException("This method is not supported when running in Kubernetes");
            }

            var netTcp = new NetTcpBinding(SecurityMode.Transport)
            {
                MaxReceivedMessageSize = int.MaxValue,
                ReceiveTimeout = TimeSpan.FromSeconds(timeoutSeconds),
                SendTimeout = TimeSpan.FromSeconds(timeoutSeconds),
                MaxConnections = 5000,
                MaxBufferPoolSize = int.MaxValue,
                MaxBufferSize = int.MaxValue,
                ListenBacklog = int.MaxValue,
                ReaderQuotas = new XmlDictionaryReaderQuotas()
                {
                    MaxArrayLength = int.MaxValue,
                    MaxStringContentLength = int.MaxValue
                },
                Security = new NetTcpSecurity()
                {
                    Mode = SecurityMode.Transport,
                    Transport = new TcpTransportSecurity()
                    {
                        ClientCredentialType = TcpClientCredentialType.Certificate
                    }
                },
                TransferMode = TransferMode.Streamed
            };

            return netTcp;
        }

        internal static AlkamiCoreWcfHostingHttpBinding HttpBinding()
        {
            return HttpBinding(DefaultTimeout, false);
        }

        internal static AlkamiCoreWcfHostingHttpBinding HttpBinding(bool https)
        {
            return HttpBinding(DefaultTimeout, https);
        }

        internal static AlkamiCoreWcfHostingHttpBinding HttpBinding(int timeoutSeconds)
        {
            return HttpBinding(timeoutSeconds, false);
        }

        internal static AlkamiCoreWcfHostingHttpBinding HttpBinding(int timeoutSeconds, bool https)
        {
            if (timeoutSeconds < 0)
                throw new ArgumentException(nameof(timeoutSeconds) + " cannot be < 0");

            var alkamiHttpBinding = new AlkamiCoreWcfHostingHttpBinding(new BasicHttpBinding(https ? BasicHttpSecurityMode.Transport : BasicHttpSecurityMode.None)
            {
                MaxReceivedMessageSize = int.MaxValue,
                ReceiveTimeout = TimeSpan.FromSeconds(timeoutSeconds),
                SendTimeout = TimeSpan.FromSeconds(timeoutSeconds),
                MaxBufferSize = int.MaxValue,
                ReaderQuotas = new XmlDictionaryReaderQuotas()
                {
                    MaxArrayLength = int.MaxValue,
                    MaxStringContentLength = int.MaxValue
                },

                TransferMode = TransferMode.Streamed
            });

            return alkamiHttpBinding;
        }
    }
}
#endif
