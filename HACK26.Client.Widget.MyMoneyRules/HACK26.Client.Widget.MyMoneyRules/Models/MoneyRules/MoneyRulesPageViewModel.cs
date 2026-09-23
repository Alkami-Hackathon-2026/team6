using Alkami.Client.Framework.Mvc;
using System.Collections.Generic;

namespace HACK26.Client.Widget.MyMoneyRules.Models.MoneyRules
{
    public class MoneyRulesPageViewModel : BaseModel
    {
        public AutomationSummaryViewModel Summary { get; set; }

        public RuleBuilderViewModel Builder { get; set; }

        public IList<RecipeViewModel> Recipes { get; set; }

        public IList<MoneyRuleViewModel> Rules { get; set; }
    }
}
