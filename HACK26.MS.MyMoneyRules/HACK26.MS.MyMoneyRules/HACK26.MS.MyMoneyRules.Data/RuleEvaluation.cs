using System;
using System.ComponentModel.DataAnnotations;

namespace HACK26.MS.MyMoneyRules.Data
{
    /// <summary>
    /// The result of evaluating one rule against one transaction event. Stored in core.UserEngineRuleEvaluations.
    /// </summary>
    public class RuleEvaluation
    {
        /// <summary>
        /// Evaluation identifier
        /// </summary>
        [Key] public int EvaluationId { get; set; }

        /// <summary>
        /// Identifier of the evaluated transaction event
        /// </summary>
        public int EventId { get; set; }

        /// <summary>
        /// Identifier of the evaluated rule
        /// </summary>
        public int RuleId { get; set; }

        /// <summary>
        /// Whether the rule matched the transaction
        /// </summary>
        public bool Matched { get; set; }

        /// <summary>
        /// Date and time of the evaluation in UTC
        /// </summary>
        public DateTime EvaluationDateUtc { get; set; }

        /// <summary>
        /// The evaluated transaction event
        /// </summary>
        public TransactionEvent Event { get; set; }

        /// <summary>
        /// The evaluated rule
        /// </summary>
        public DecisionRule Rule { get; set; }
    }
}
