using Alkami.Data.Validations;
using Alkami.Exceptions;
using Alkami.MicroServices.Settings.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alkami.TrackableObjects.Exceptions
{
    public class ItemUpdateFailedException : Exception
    {
        public Item Item { get; private set; }
        public IEnumerable<ValidationResult> ValidationResults { get; set; }

        public ItemUpdateFailedException(Item item, IEnumerable<ValidationResult> validationResults)
            : base("The item could not be updated due to an error. Inspect the ValidationResults collection for more information.")
        {
            Item = item;
            ValidationResults = validationResults;
        }
    }
}