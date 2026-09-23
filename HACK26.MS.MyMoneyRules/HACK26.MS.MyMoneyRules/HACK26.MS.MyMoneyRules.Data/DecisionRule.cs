using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HACK26.MS.MyMoneyRules.Data
{
    [Table("Rules")]
    public class DecisionRule
    {
        [Key] public int RuleId { get; set; }
        public int UserId { get; set; }
        [MaxLength(200)] public string RuleName { get; set; } = string.Empty;
        public int Priority { get; set; } = 100;
        public bool IsActive { get; set; } = true;
        public ICollection<RuleTrigger> Triggers { get; set; } = new List<RuleTrigger>();
        public ICollection<ConditionGroup> ConditionGroups { get; set; } = new List<ConditionGroup>();
        public ICollection<RuleAction> Actions { get; set; } = new List<RuleAction>();
    }
}
