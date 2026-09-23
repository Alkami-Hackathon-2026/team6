using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HACK26.MS.MyMoneyRules.Data
{
    [Table("Conditions")]
    public class RuleCondition
    {
        [Key] public int ConditionId { get; set; }
        public int ConditionGroupId { get; set; }
        [MaxLength(100)] public string FieldName { get; set; } = string.Empty;
        [MaxLength(20)] public string Operator { get; set; } = string.Empty;
        [MaxLength(500)] public string Value { get; set; } = string.Empty;
        public ConditionGroup ConditionGroup { get; set; }
    }
}
