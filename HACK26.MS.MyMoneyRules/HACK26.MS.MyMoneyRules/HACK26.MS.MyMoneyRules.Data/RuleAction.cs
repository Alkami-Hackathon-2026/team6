using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HACK26.MS.MyMoneyRules.Data
{
    [Table("Actions")]
    public class RuleAction
    {
        [Key] public int ActionId { get; set; }
        public int RuleId { get; set; }
        [MaxLength(100)] public string ActionType { get; set; } = string.Empty;
        [MaxLength(1000)] public string ActionValue { get; set; }
        public DecisionRule Rule { get; set; }
    }
}
