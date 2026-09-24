using Alkami.Contracts;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace HACK26.MS.MyMoneyRules.Contracts.Sorters
{
    /// <summary>
    /// Sort order for <see cref="Data.RuleEvaluation"/> records
    /// </summary>
    [DataContract(IsReference = true)]
    public class RuleEvaluationSorter : ISortOrder<RuleEvaluationFields>
    {
        /// <inheritdoc />
        [DataMember(EmitDefaultValue = false)]
        public bool Ascending { get; set; }

        /// <inheritdoc />
        [DataMember(EmitDefaultValue = false)]
        public List<RuleEvaluationFields> OrderByFields { get; set; }
    }
}
