using System;
using System.ComponentModel.DataAnnotations;

namespace HACK26.MS.MyMoneyRules.Data
{
    /// <summary>
    /// A record of an action run for a matched rule. Stored in core.UserEngineActionExecutions.
    /// </summary>
    public class ActionExecution
    {
        /// <summary>
        /// Action execution identifier
        /// </summary>
        [Key] public int ActionExecutionId { get; set; }

        /// <summary>
        /// Identifier of the rule evaluation that caused the action
        /// </summary>
        public int EvaluationId { get; set; }

        /// <summary>
        /// Identifier of the action
        /// </summary>
        public int ActionId { get; set; }

        /// <summary>
        /// Execution status: PENDING, SUCCESS or FAILED. Defaults to PENDING.
        /// </summary>
        [MaxLength(20)] public string Status { get; set; } = "PENDING";

        /// <summary>
        /// Date and time the status was last set, in UTC
        /// </summary>
        public DateTime ExecutionDateUtc { get; set; }

        /// <summary>
        /// The rule evaluation that caused the action
        /// </summary>
        public RuleEvaluation Evaluation { get; set; }

        /// <summary>
        /// The action that was run
        /// </summary>
        public RuleAction Action { get; set; }
    }
}
