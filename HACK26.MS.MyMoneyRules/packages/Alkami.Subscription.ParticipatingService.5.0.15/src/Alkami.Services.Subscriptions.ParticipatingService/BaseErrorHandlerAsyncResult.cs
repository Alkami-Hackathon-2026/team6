#if NETFRAMEWORK
using System;
using System.Threading;

namespace Alkami.Services.Subscriptions.ParticipatingService
{
    internal class BaseErrorHandlerAsyncResult : IAsyncResult
    {
        IAsyncResult originalResult;
        ValidationUserState validationUserState;


        public BaseErrorHandlerAsyncResult(IAsyncResult originalResult, ValidationUserState validationUserState)
        {
            this.originalResult = originalResult;
            this.validationUserState = validationUserState;
        }

        public bool IsCompleted
        {
            get
            {
                return this.originalResult.IsCompleted;
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                return this.originalResult.AsyncWaitHandle;
            }
        }

        public object AsyncState
        {
            get
            {
                return this.validationUserState.OriginalUserState;
            }
        }

        public bool CompletedSynchronously
        {
            get
            {
                return this.originalResult.CompletedSynchronously;
            }
        }

        internal ValidationUserState ValidationUserState
        {
            get { return this.validationUserState; }
        }

        internal IAsyncResult OriginalAsyncResult
        {
            get { return this.originalResult; }
        }
    }
}
#endif