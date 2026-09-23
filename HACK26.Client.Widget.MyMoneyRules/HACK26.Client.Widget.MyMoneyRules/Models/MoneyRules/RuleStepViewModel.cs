using Alkami.Client.Framework.Mvc;

namespace HACK26.Client.Widget.MyMoneyRules.Models.MoneyRules
{
    public class RuleStepViewModel : BaseModel
    {
        public string Label { get; set; }

        public string PrimaryText { get; set; }

        public string SecondaryText { get; set; }

        public bool IsOutcomeStep { get; set; }
    }
}
