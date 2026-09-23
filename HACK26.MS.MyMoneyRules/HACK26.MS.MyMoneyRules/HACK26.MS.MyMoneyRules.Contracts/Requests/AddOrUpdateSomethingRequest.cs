using Alkami.Contracts;
using HACK26.MS.MyMoneyRules.Data;
using System.Runtime.Serialization;

namespace HACK26.MS.MyMoneyRules.Contracts.Requests
{
    /// <summary>
    /// TODO: add xml doc comments
    /// </summary>
    [DataContract(IsReference = true)]
    public class AddOrUpdateSomethingRequest : BaseCreateOrUpdateRequest<CustomDataObject>
    {

    }
}