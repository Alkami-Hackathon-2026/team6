using Alkami.Contracts;
using HACK26.MS.MyMoneyRules.Contracts.Filters;
using HACK26.MS.MyMoneyRules.Contracts.Mappers;
using HACK26.MS.MyMoneyRules.Contracts.Sorters;
using System.Runtime.Serialization;

namespace HACK26.MS.MyMoneyRules.Contracts.Requests
{
    /// <summary>
    /// TODO: add xml doc comments
    /// </summary>
    [DataContract(IsReference = true)]
    public class GetSomethingRequest : BaseGetRequest<CustomDataObjectFilter, CustomDataObjectMapper, CustomDataObjectSorter>
    {

    }
}
