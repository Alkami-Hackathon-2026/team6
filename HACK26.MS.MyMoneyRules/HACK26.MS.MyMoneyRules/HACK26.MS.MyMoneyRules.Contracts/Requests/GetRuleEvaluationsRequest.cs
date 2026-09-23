using Alkami.Contracts;
using HACK26.MS.MyMoneyRules.Contracts.Filters;
using HACK26.MS.MyMoneyRules.Contracts.Mappers;
using HACK26.MS.MyMoneyRules.Contracts.Sorters;
using System.Runtime.Serialization;

namespace HACK26.MS.MyMoneyRules.Contracts.Requests
{
    /// <summary>
    /// Request to retrieve <see cref="Data.RuleEvaluation"/> history
    /// </summary>
    [DataContract(IsReference = true)]
    public class GetRuleEvaluationsRequest : BaseGetRequest<RuleEvaluationFilter, RuleEvaluationMapper, RuleEvaluationSorter>
    {
    }
}
