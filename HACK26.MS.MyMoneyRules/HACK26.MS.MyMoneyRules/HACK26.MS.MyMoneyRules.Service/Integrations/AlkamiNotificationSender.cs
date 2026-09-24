using Alkami.Contracts;
using Alkami.MicroServices.Notifications.Contracts.Requests;
using Alkami.MicroServices.Notifications.Data;
using Alkami.MicroServices.Notifications.Service.Client;
using Common.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using AlkamiNotification = Alkami.MicroServices.Notifications.Data.Notification.Notification;

namespace HACK26.MS.MyMoneyRules.Service.Integrations
{
    /// <summary>
    /// <see cref="INotificationSender"/> backed by the Alkami notifications service
    /// </summary>
    public class AlkamiNotificationSender : INotificationSender
    {
        private static readonly ILog Logger = LogManager.GetLogger<AlkamiNotificationSender>();

        private const string OriginatorName = "My Money Rules";
        private const string Summary = "My Money Rules alert";

        private readonly NotificationServiceClient _client;

        /// <summary>
        /// Create a sender using a default <see cref="NotificationServiceClient"/>
        /// </summary>
        public AlkamiNotificationSender() : this(new NotificationServiceClient())
        {
        }

        /// <summary>
        /// Create a sender using the given client
        /// </summary>
        public AlkamiNotificationSender(NotificationServiceClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc />
        public async Task<bool> SendAsync(BaseRequest request, UserContact contact, string channel, string message, CancellationToken cancellationToken)
        {
            if (contact == null || string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (!TryResolveDestination(request, contact, channel, out var mediumType, out var destination))
            {
                Logger.Warn($"{nameof(AlkamiNotificationSender)} | No destination for channel [{channel}] for user [{contact.UserId}]");
                return false;
            }

            var sendRequest = new SendBasicNotificationRequest
            {
                Notification = new AlkamiNotification
                {
                    OriginatorName = OriginatorName,
                    MediumType = mediumType,
                    DestinationAddress = destination,
                    Summary = Summary,
                    Message = message
                }
            };
            sendRequest.CopyBaseFrom(request);

            var response = await _client.SendBasicNotificationAsync(sendRequest).ConfigureAwait(false);

            if (response == null || response.HasError)
            {
                Logger.Error($"{nameof(AlkamiNotificationSender)} | Failed sending [{mediumType}] notification to user [{contact.UserId}] | {response?.SystemMessage}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Maps the rule channel to a medium and destination; falls back to email then SMS when the preferred channel has no address
        /// </summary>
        private static bool TryResolveDestination(BaseRequest request, UserContact contact, string channel, out MediumType mediumType, out string destination)
        {
            switch ((channel ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "email":
                    if (!string.IsNullOrWhiteSpace(contact.Email))
                    {
                        mediumType = MediumType.Email;
                        destination = contact.Email;
                        return true;
                    }
                    break;

                case "sms":
                case "text":
                    if (!string.IsNullOrWhiteSpace(contact.MobilePhone))
                    {
                        mediumType = MediumType.Sms;
                        destination = contact.MobilePhone;
                        return true;
                    }
                    break;

                case "push":
                    if (request?.UserIdentifier != null)
                    {
                        mediumType = MediumType.Push;
                        destination = request.UserIdentifier.Value.ToString();
                        return true;
                    }
                    break;
            }

            if (!string.IsNullOrWhiteSpace(contact.Email))
            {
                mediumType = MediumType.Email;
                destination = contact.Email;
                return true;
            }

            if (!string.IsNullOrWhiteSpace(contact.MobilePhone))
            {
                mediumType = MediumType.Sms;
                destination = contact.MobilePhone;
                return true;
            }

            mediumType = MediumType.Unknown;
            destination = null;
            return false;
        }
    }
}
