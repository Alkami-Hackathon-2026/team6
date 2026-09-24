using Alkami.Contracts;
using System.Runtime.Serialization;

namespace HACK26.MS.MyMoneyRules.Contracts.Mappers
{
    /// <summary>
    /// Controls which child collections of <see cref="Data.DecisionRule"/> are populated
    /// </summary>
    [DataContract(IsReference = true)]
    public class DecisionRuleMapper : IMapping
    {
        /// <summary>
        /// Include <see cref="Data.RuleTrigger"/> records
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public bool? IncludeTriggers { get; set; }

        /// <summary>
        /// Include <see cref="Data.ConditionGroup"/> and <see cref="Data.RuleCondition"/> records
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public bool? IncludeConditionGroups { get; set; }

        /// <summary>
        /// Include <see cref="Data.RuleAction"/> records
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public bool? IncludeActions { get; set; }
    }
}
