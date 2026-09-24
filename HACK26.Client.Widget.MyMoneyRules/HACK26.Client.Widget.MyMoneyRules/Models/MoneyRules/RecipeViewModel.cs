using Alkami.Client.Framework.Mvc;

namespace HACK26.Client.Widget.MyMoneyRules.Models.MoneyRules
{
    public class RecipeViewModel : BaseModel
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string AccentClass { get; set; }

        public string BuilderName { get; set; }

        public string BuilderWhen { get; set; }

        public string BuilderWhenDetail { get; set; }

        public string BuilderIf { get; set; }

        public string BuilderIfDetail { get; set; }

        public string BuilderThen { get; set; }

        public string BuilderThenDetail { get; set; }
    }
}
