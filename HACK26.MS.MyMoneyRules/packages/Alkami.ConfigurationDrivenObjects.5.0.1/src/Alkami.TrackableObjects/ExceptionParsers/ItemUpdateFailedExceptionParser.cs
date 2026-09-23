using Alkami.Exceptions;
using Alkami.TrackableObjects.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alkami.TrackableObjects.ExceptionParsers
{
    public class ItemUpdateFailedExceptionParser : IAlkamiExceptionParser
    {
        static ItemUpdateFailedExceptionParser()
        {
            AlkamiException.RegisterHandler(new ItemUpdateFailedExceptionParser());
        }

        public Type Handles
        {
            get
            {
                return typeof(ItemUpdateFailedException);
            }
        }

        public AlkamiException Parse(Exception input)
        {
            var updateFailedException = input as ItemUpdateFailedException;

            if (updateFailedException == null)
            {
                return new AlkamiException(input, Data.Validations.ErrorCode.SystemNonFatalError, Data.Validations.SubCode.BadRequest, input.Message);
            }
            else
            {
                var errors = updateFailedException.ValidationResults.Select(x => x.Message);
                var formattedMessage = string.Format("One or more itemSettings were unable to be saved successfully - the following errors were encountered: {0}", string.Join(Environment.NewLine, errors));

                return new AlkamiException(input, Data.Validations.ErrorCode.SystemNonFatalError, Data.Validations.SubCode.BadRequest, formattedMessage);
            }
        }
    }
}
