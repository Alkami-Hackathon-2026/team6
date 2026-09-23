using System.Runtime.Serialization;

namespace HACK26.MS.MyMoneyRules.Contracts
{
    /// <summary>
    /// Sortable fields for <see cref="Data.DecisionRule"/>
    /// </summary>
    [DataContract]
    public enum DecisionRuleFields
    {
        /// <summary>
        /// Rule identifier
        /// </summary>
        [EnumMember]
        RuleId = 0,

        /// <summary>
        /// Rule name
        /// </summary>
        [EnumMember]
        RuleName = 1,

        /// <summary>
        /// Rule priority
        /// </summary>
        [EnumMember]
        Priority = 2,

        /// <summary>
        /// Rule active flag
        /// </summary>
        [EnumMember]
        IsActive = 3
    }
}
