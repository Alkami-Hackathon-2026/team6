using Alkami.Client.Framework.Mvc;

namespace HACK26.Client.Widget.MyMoneyRules.Models.MoneyRules
{
    public class MoneyRuleViewModel : BaseModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string TriggerText { get; set; }

        public string ActionText { get; set; }

        public string Impact { get; set; }

        public bool IsActive { get; set; }
    }
}
