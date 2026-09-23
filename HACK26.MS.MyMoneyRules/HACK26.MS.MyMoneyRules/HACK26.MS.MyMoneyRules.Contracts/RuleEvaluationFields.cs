using System.Runtime.Serialization;

namespace HACK26.MS.MyMoneyRules.Contracts
{
    /// <summary>
    /// Sortable fields for <see cref="Data.RuleEvaluation"/>
    /// </summary>
    [DataContract]
    public enum RuleEvaluationFields
    {
        /// <summary>
        /// Evaluation identifier
        /// </summary>
        [EnumMember]
        EvaluationId = 0,

        /// <summary>
        /// Evaluation date
        /// </summary>
        [EnumMember]
        EvaluationDateUtc = 1,

        /// <summary>
        /// Rule identifier
        /// </summary>
        [EnumMember]
        RuleId = 2,

        /// <summary>
        /// Event identifier
        /// </summary>
        [EnumMember]
        EventId = 3
    }
}
