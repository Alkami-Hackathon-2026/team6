using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HACK26.MS.MyMoneyRules.Data
{
    [Table("Triggers")]
    public class RuleTrigger
    {
        [Key] public int TriggerId { get; set; }
        public int RuleId { get; set; }
        [MaxLength(100)] public string FieldName { get; set; } = "transactionOccurred";
        [MaxLength(20)] public string Operator { get; set; } = "=";
        [MaxLength(500)] public string Value { get; set; } = "true";
        public DecisionRule Rule { get; set; }
    }
}
