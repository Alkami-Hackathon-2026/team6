using Alkami.Client.Framework.Mvc;

namespace HACK26.Client.Widget.MyMoneyRules.Models.MoneyRules
{
    public class AutomationSummaryViewModel : BaseModel
    {
        public string Title { get; set; }

        public string AmountMoved { get; set; }

        public string AmountMovedDescription { get; set; }

        public int ActiveRules { get; set; }

        public int ActionsCompleted { get; set; }
    }
}
