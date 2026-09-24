using System.ComponentModel.DataAnnotations;

namespace HACK26.MS.MyMoneyRules.Data
{
    /// <summary>
    /// The result of one condition during a rule evaluation. Stored in core.UserEngineConditionEvaluations.
    /// </summary>
    public class ConditionEvaluation
    {
        /// <summary>
        /// Condition evaluation identifier
        /// </summary>
        [Key] public int ConditionEvaluationId { get; set; }

        /// <summary>
        /// Identifier of the rule evaluation this result belongs to
        /// </summary>
        public int EvaluationId { get; set; }

        /// <summary>
        /// Identifier of the evaluated condition
        /// </summary>
        public int ConditionId { get; set; }

        /// <summary>
        /// Value of the field on the transaction
        /// </summary>
        [MaxLength(500)] public string ActualValue { get; set; }

        /// <summary>
        /// Value the condition expected
        /// </summary>
        [MaxLength(500)] public string ExpectedValue { get; set; }

        /// <summary>
        /// Whether the condition passed
        /// </summary>
        public bool Result { get; set; }

        /// <summary>
        /// The rule evaluation this result belongs to
        /// </summary>
        public RuleEvaluation Evaluation { get; set; }

        /// <summary>
        /// The evaluated condition
        /// </summary>
        public RuleCondition Condition { get; set; }
    }
}
