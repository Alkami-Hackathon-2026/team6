using Alkami.Contracts;
using HACK26.MS.MyMoneyRules.Data;
using System.Runtime.Serialization;

namespace HACK26.MS.MyMoneyRules.Contracts.Requests
{
    /// <summary>
    /// Request to create or update <see cref="DecisionRule"/> records, including their triggers, condition groups, conditions and actions
    /// </summary>
    [DataContract(IsReference = true)]
    public class AddOrUpdateDecisionRuleRequest : BaseCreateOrUpdateRequest<DecisionRule>
    {
    }
}
