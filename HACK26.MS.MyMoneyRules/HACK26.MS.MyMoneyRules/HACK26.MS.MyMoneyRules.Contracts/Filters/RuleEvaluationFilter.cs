using Alkami.Contracts;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace HACK26.MS.MyMoneyRules.Contracts.Filters
{
    /// <summary>
    /// Filter for retrieving <see cref="Data.RuleEvaluation"/> records
    /// </summary>
    [DataContract(IsReference = true)]
    public class RuleEvaluationFilter : IFilter
    {
        /// <inheritdoc />
        [DataMember(EmitDefaultValue = true)]
        public List<long> Ids { get; set; }

        /// <summary>
        /// Evaluation identifiers to match
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public List<int> EvaluationIds { get; set; }

        /// <summary>
        /// Rule identifiers to match
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public List<int> RuleIds { get; set; }

        /// <summary>
        /// Transaction event identifiers to match
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public List<int> EventIds { get; set; }

        /// <summary>
        /// Matched flag to match
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public bool? Matched { get; set; }

        /// <summary>
        /// Inclusive start of evaluation date range
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public DateTime? FromDateUtc { get; set; }

        /// <summary>
        /// Inclusive end of evaluation date range
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public DateTime? ToDateUtc { get; set; }
    }
}
