using Alkami.Contracts;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace HACK26.MS.MyMoneyRules.Contracts.Filters
{
    /// <summary>
    /// Filter for retrieving <see cref="Data.DecisionRule"/> records
    /// </summary>
    [DataContract(IsReference = true)]
    public class DecisionRuleFilter : IFilter
    {
        /// <inheritdoc />
        [DataMember(EmitDefaultValue = true)]
        public List<long> Ids { get; set; }

        /// <summary>
        /// Rule identifiers to match
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public List<int> RuleIds { get; set; }

        /// <summary>
        /// Owning user identifier
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public int? UserId { get; set; }

        /// <summary>
        /// Active flag to match
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public bool? IsActive { get; set; }

        /// <summary>
        /// Partial rule name to match
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public string PartialRuleName { get; set; }
    }
}
