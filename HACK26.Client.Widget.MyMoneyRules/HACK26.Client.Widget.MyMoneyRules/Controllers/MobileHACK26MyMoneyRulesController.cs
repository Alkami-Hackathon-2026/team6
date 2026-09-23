using Alkami.Client.Framework.Mvc;
using Alkami.Common;
using Alkami.Security.Common.Claims;
using Common.Logging;
using System;
using System.Web.Mvc;

namespace HACK26.Client.Widget.MyMoneyRules.Controllers
{
    [ClaimsAuthorizationFilter(PermissionNames.NoPermissions)]
    public class MobileHACK26MyMoneyRulesController : BaseController
    {
        /// <summary>
        /// Gets logger
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger<MobileHACK26MyMoneyRulesController>();

        /// <summary>
        /// Standard widget entry route
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            try
            {
                Logger.DebugFormat("[GET] Controller/Index");
                return View("~/Views/HACK26MyMoneyRules/Mobile/Index.cshtml");
            }
            catch (Exception e)
            {
                Logger.Error("Error [GET] Controller/Index", e);
                return View("~/Views/HACK26MyMoneyRules/Mobile/Error.cshtml");
            }
        }
    }
}