using Alkami.Client.Framework.Mvc;
using System.Collections.Generic;

namespace HACK26.Client.Widget.MyMoneyRules.Models.MoneyRules
{
    public class RuleBuilderViewModel : BaseModel
    {
        public string Header { get; set; }

        public string DefaultRuleName { get; set; }

        public IList<RuleStepViewModel> Steps { get; set; }
    }
}
