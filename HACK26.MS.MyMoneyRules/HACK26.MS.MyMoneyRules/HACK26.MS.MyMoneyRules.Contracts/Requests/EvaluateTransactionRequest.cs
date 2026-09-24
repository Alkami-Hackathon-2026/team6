using Alkami.Contracts;
using HACK26.MS.MyMoneyRules.Data;
using System.Runtime.Serialization;

namespace HACK26.MS.MyMoneyRules.Contracts.Requests
{
    /// <summary>
    /// Request to evaluate a <see cref="Data.TransactionEvent"/> against the active rules.
    /// The rules evaluated are those owned by the inherited <see cref="BaseRequest.UserId"/>.
    /// </summary>
    [DataContract(IsReference = true)]
    public class EvaluateTransactionRequest : BaseRequest
    {
        /// <summary>
        /// Transaction event to evaluate
        /// </summary>
        [DataMember(EmitDefaultValue = true)]
        public TransactionEvent TransactionEvent { get; set; }
    }
}
