using Alkami.Contracts;
using System.Runtime.Serialization;

namespace HACK26.MS.MyMoneyRules.Contracts.Mappers
{
    /// <summary>
    /// Controls which related data of <see cref="Data.RuleEvaluation"/> is populated
    /// </summary>
    [DataContract(IsReference = true)]
    public class RuleEvaluationMapper : IMapping
    {
        /// <summary>
        /// Include the <see cref="Data.TransactionEvent"/>
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public bool? IncludeEvent { get; set; }

        /// <summary>
        /// Include the <see cref="Data.DecisionRule"/>
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public bool? IncludeRule { get; set; }
    }
}
