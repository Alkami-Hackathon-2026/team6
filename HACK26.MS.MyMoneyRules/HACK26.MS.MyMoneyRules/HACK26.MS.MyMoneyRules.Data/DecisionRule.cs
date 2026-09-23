using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HACK26.MS.MyMoneyRules.Data
{
    /// <summary>
    /// A user-defined rule that is evaluated against transaction events. Stored in core.UserEngineRules.
    /// </summary>
    [Table("Rules")]
    public class DecisionRule
    {
        /// <summary>
        /// Rule identifier
        /// </summary>
        [Key] public int RuleId { get; set; }

        /// <summary>
        /// Identifier of the user who owns the rule
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Display name of the rule
        /// </summary>
        [MaxLength(200)] public string RuleName { get; set; } = string.Empty;

        /// <summary>
        /// Evaluation order; lower values are evaluated first. Defaults to 100.
        /// </summary>
        public int Priority { get; set; } = 100;

        /// <summary>
        /// Whether the rule is evaluated. Inactive rules are ignored.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Triggers that must all pass before the rule's conditions are evaluated
        /// </summary>
        public ICollection<RuleTrigger> Triggers { get; set; } = new List<RuleTrigger>();

        /// <summary>
        /// Condition groups that determine whether the rule matches
        /// </summary>
        public ICollection<ConditionGroup> ConditionGroups { get; set; } = new List<ConditionGroup>();

        /// <summary>
        /// Actions performed when the rule matches
        /// </summary>
        public ICollection<RuleAction> Actions { get; set; } = new List<RuleAction>();
    }
}
