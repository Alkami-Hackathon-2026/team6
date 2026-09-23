using Alkami.Client.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HACK26.Client.Widget.MyMoneyRules
{
    public class WidgetDescription : Alkami.Client.Framework.Mvc.WidgetDescription
    {
        private const string _name = "HACK26MyMoneyRules";

        public override string Name
        {
            get { return _name; }
        }

        public override string Title
        {
            get { return _name; }
        }
    }
}
