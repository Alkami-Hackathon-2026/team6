using Alkami.Contracts;
using HACK26.MS.MyMoneyRules.Data;
using System.Runtime.Serialization;

namespace HACK26.MS.MyMoneyRules.Contracts.Responses
{
    /// <summary>
    /// Response containing <see cref="FieldDefinition"/> records
    /// </summary>
    [DataContract(IsReference = true)]
    public class FieldDefinitionResponse : BaseResponse<FieldDefinition>
    {
    }
}
