using System.ComponentModel.DataAnnotations;

namespace HACK26.MS.MyMoneyRules.Data
{
    public class ConditionEvaluation
    {
        [Key] public int ConditionEvaluationId { get; set; }
        public int EvaluationId { get; set; }
        public int ConditionId { get; set; }
        [MaxLength(500)] public string ActualValue { get; set; }
        [MaxLength(500)] public string ExpectedValue { get; set; }
        public bool Result { get; set; }
        public RuleEvaluation Evaluation { get; set; }
        public RuleCondition Condition { get; set; }
    }
}
