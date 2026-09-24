using Alkami.Contracts;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace HACK26.MS.MyMoneyRules.Contracts.Requests
{
    /// <summary>
    /// Request to delete <see cref="Data.DecisionRule"/> records
    /// </summary>
    [DataContract(IsReference = true)]
    public class DeleteDecisionRulesRequest : BaseRequest
    {
        /// <summary>
        /// Rule identifiers to delete
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public List<int> RuleIds { get; set; } = new List<int>();
    }
}
