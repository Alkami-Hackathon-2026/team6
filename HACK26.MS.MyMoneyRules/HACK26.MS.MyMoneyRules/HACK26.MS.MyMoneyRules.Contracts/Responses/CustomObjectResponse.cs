using Alkami.Contracts;
using HACK26.MS.MyMoneyRules.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HACK26.MS.MyMoneyRules.Contracts.Responses
{
    /// <summary>
    /// TODO: add xml doc comments
    /// </summary>
    [DataContract(IsReference = true)]
    public class CustomObjectResponse : BaseResponse<CustomDataObject>
    {

    }
}