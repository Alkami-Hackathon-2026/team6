#if NETFRAMEWORK
using System;

namespace Alkami.Services.Subscriptions.ParticipatingService
{
    internal class ValidationUserState
    {
        public BaseErrorHandlerResult ValidationResult { get; set; }
        public AsyncCallback OriginalUserCallback { get; set; }
        public object OriginalUserState { get; set; }
    }
}
#endif
