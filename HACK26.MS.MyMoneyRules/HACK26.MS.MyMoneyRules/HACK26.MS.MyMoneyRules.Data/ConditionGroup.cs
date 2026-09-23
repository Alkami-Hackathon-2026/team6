using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HACK26.MS.MyMoneyRules.Data
{
    public class ConditionGroup
    {
        [Key] public int ConditionGroupId { get; set; }
        public int RuleId { get; set; }
        public int? ParentConditionGroupId { get; set; }
        [MaxLength(3)] public string LogicOperator { get; set; } = "AND";
        public DecisionRule Rule { get; set; }
        public ConditionGroup ParentConditionGroup { get; set; }
        public ICollection<ConditionGroup> ChildGroups { get; set; } = new List<ConditionGroup>();
        public ICollection<RuleCondition> Conditions { get; set; } = new List<RuleCondition>();
    }
}
