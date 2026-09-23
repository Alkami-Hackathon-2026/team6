using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HACK26.MS.MyMoneyRules.Data
{
    /// <summary>
    /// A group of conditions combined with AND or OR. Groups can be nested. Stored in core.UserEngineConditionGroups.
    /// </summary>
    public class ConditionGroup
    {
        /// <summary>
        /// Condition group identifier
        /// </summary>
        [Key] public int ConditionGroupId { get; set; }

        /// <summary>
        /// Identifier of the rule the group belongs to
        /// </summary>
        public int RuleId { get; set; }

        /// <summary>
        /// Identifier of the parent group; null for a top-level group
        /// </summary>
        public int? ParentConditionGroupId { get; set; }

        /// <summary>
        /// How the group's conditions and child groups are combined: AND or OR. Defaults to AND.
        /// </summary>
        [MaxLength(3)] public string LogicOperator { get; set; } = "AND";

        /// <summary>
        /// The rule the group belongs to
        /// </summary>
        public DecisionRule Rule { get; set; }

        /// <summary>
        /// The parent group, when this group is nested
        /// </summary>
        public ConditionGroup ParentConditionGroup { get; set; }

        /// <summary>
        /// Nested child groups
        /// </summary>
        public ICollection<ConditionGroup> ChildGroups { get; set; } = new List<ConditionGroup>();

        /// <summary>
        /// Conditions in the group
        /// </summary>
        public ICollection<RuleCondition> Conditions { get; set; } = new List<RuleCondition>();
    }
}
