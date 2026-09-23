using System;
using System.ComponentModel.DataAnnotations;

namespace HACK26.MS.MyMoneyRules.Data
{
    public class ActionExecution
    {
        [Key] public int ActionExecutionId { get; set; }
        public int EvaluationId { get; set; }
        public int ActionId { get; set; }
        [MaxLength(20)] public string Status { get; set; } = "PENDING";
        public DateTime ExecutionDateUtc { get; set; }
        public RuleEvaluation Evaluation { get; set; }
        public RuleAction Action { get; set; }
    }
}
