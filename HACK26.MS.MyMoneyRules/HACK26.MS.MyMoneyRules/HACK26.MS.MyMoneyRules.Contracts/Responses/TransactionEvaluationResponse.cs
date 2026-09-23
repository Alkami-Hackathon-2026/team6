using Alkami.Contracts;
using HACK26.MS.MyMoneyRules.Data;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace HACK26.MS.MyMoneyRules.Contracts.Responses
{
    /// <summary>
    /// Response containing the <see cref="RuleEvaluation"/> results for an evaluated <see cref="TransactionEvent"/>
    /// </summary>
    [DataContract(IsReference = true)]
    public class TransactionEvaluationResponse : BaseResponse<RuleEvaluation>
    {
        /// <summary>
        /// The evaluated transaction event
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public TransactionEvent TransactionEvent { get; set; }

        /// <summary>
        /// Per-condition results for each evaluation
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public List<ConditionEvaluation> ConditionEvaluations { get; set; } = new List<ConditionEvaluation>();

        /// <summary>
        /// Actions executed for matched rules
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public List<ActionExecution> ActionExecutions { get; set; } = new List<ActionExecution>();
    }
}
