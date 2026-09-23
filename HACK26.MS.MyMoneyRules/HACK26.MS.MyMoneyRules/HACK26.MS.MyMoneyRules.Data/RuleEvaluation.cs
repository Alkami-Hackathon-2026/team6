using System;
using System.ComponentModel.DataAnnotations;

namespace HACK26.MS.MyMoneyRules.Data
{
    public class RuleEvaluation
    {
        [Key] public int EvaluationId { get; set; }
        public int EventId { get; set; }
        public int RuleId { get; set; }
        public bool Matched { get; set; }
        public DateTime EvaluationDateUtc { get; set; }
        public TransactionEvent Event { get; set; }
        public DecisionRule Rule { get; set; }
    }
}
