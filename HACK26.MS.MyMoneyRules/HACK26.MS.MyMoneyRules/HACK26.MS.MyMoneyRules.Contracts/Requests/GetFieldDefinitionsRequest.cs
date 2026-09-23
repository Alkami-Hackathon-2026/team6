using Alkami.Contracts;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace HACK26.MS.MyMoneyRules.Contracts.Requests
{
    /// <summary>
    /// Request to retrieve <see cref="Data.FieldDefinition"/> records
    /// </summary>
    [DataContract(IsReference = true)]
    public class GetFieldDefinitionsRequest : BaseRequest
    {
        /// <summary>
        /// Field names to match; all fields are returned when empty
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public List<string> FieldNames { get; set; } = new List<string>();
    }
}
