using Alkami.Client.Framework.Mvc;
using Alkami.Common;
using Alkami.MicroServices.Accounts.Data;
using Alkami.Security.Common.Claims;
using Common.Logging;
using HACK26.Client.Widget.MyMoneyRules.Helpers;
using HACK26.Client.Widget.MyMoneyRules.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Mvc;
using WebToolkit;
using Account = Alkami.MicroServices.Accounts.Data.Account;
using Exception = System.Exception;

// used when widget setting is uncommented below.
// using Alkami.Client.Framework.Utility;


namespace HACK26.Client.Widget.MyMoneyRules.Controllers
{
    public class HACK26MyMoneyRulesController : BaseController
    {

        ///// <summary>
        ///// https://{bank url}/afx/v2/compliance/fdic-configuration 
        ///// The FDIC Compliance Client Endpoint allows external applications to retrieve the current status of FDIC signage settings for all designated display locations. 
        ///// These include login pages, headers, footers, and inline flows for both desktop and mobile platforms. 
        ///// The endpoint ensures dynamic configuration retrieval, helping maintain compliance with regulatory requirements.
        ///// </summary>
        ///// <returns>FDICComplianceConfigs</returns>
        //public async Task<FdicComplianceConfigsResponse> GetFDICSignageConfigurationAfx()
        //{
        //    var request = new ProxyRequest
        //    {
        //        HttpMethod = HttpMethod.Get.ToString(),
        //        RequestUri = new Uri("https://developer.dev.alkamitech.com/afx/v2/compliance/fdic-configuration")
        //    };

        //    this.AugmentRequest(request);
        //    var response = await GenericProxyFactory().ExecuteRequestAsync(request);
        //    var fdicCompliance = JsonConvert.DeserializeObject<FdicComplianceConfigsResponse>(response.Body);
        //    return fdicCompliance;
        //}

        /// <summary>
        /// Gets the logger
        /// </summary>
        private static readonly ILog Logger = LogManager.GetLogger<HACK26MyMoneyRulesController>();

        // please add this logPrefix to the beginging of all your log statements
        string logPrefix = "HACK26MyMoneyRules";

        private AlkamiHelpers alkamiHelpers = new AlkamiHelpers();

        public ActionResult Index()
        {
            try
            {
                Logger.DebugFormat($" {logPrefix} [GET] Controller/Index");

                return View("Index");
            }
            catch (Exception e)
            {
                Logger.Error("Error [GET] Controller/Index", e);
                return View("Error");
            }
        }

        public ActionResult Accounts()
        {
            var userId = this.CurrentUser.Id;
            var Accounts = GetAccounts(userId);

            var model = new AccountsModel();
            model.Accounts = Accounts;
            // model.Accounts.FirstOrDefault().Id
            // model.Accounts.FirstOrDefault().AvailableBalance
            // model.Accounts.FirstOrDefault().Number
            // model.Accounts.FirstOrDefault().AccountType
            return View("Accounts", model);
        }

        private List<Account> GetAccounts(long userId)
        {
            Logger.DebugFormat($" {logPrefix} GetAccounts Controller method called userId {userId}");
            // Get User Request to call security service
            var getUserRequest = alkamiHelpers.GetUserRequest(userId);

            Logger.DebugFormat($" {logPrefix} GetUserRequest returned for user {userId}");

            // Augment request will add bank Identifier and other important information
            this.AugmentRequest(getUserRequest);

            Logger.DebugFormat($" {logPrefix} userRequest Augmented");

            // Use the User request to pull back the UserAccounts
            var userAccounts = alkamiHelpers.GetUserAccounts(getUserRequest);

            // Extract active account Ids from the user accounts
            var accountIds = userAccounts?.Where(y => !y.Deleted)?.Select(x => x.AccountId) ?? new List<long>();
            Logger.DebugFormat($" {logPrefix} GetUserAccounts returned {String.Join(",", accountIds)}");

            // Further information on accounts can be obtained using the account microservice
            var accountFormat = BankSettings.GetSettingOrDefault<string>(BankSettingName.JoinAccountHolderNumberFormatString, "{0}{1}");
            Logger.DebugFormat($" {logPrefix} accountFormat returned {accountFormat}");

            var accountRequest = alkamiHelpers.GenerateAccountRequest(accountIds, accountFormat);
            Logger.DebugFormat($" {logPrefix} GenerateAccountRequest returned for user {userId}");

            // Augment request will add bank Identifier an other important information
            this.AugmentRequest(accountRequest);
            Logger.DebugFormat($" {logPrefix} accountRequest Augmented");

            // The Alkami Account Identifier Guid can be obtained from the accounts response
            var accountResponse = alkamiHelpers.CallAccountService(accountRequest);
            Logger.DebugFormat($" {logPrefix} CallAccountService returned for user {userId} accounts {String.Join(",", accountIds)}");

            // examples of what is on the account object, alot more information is available on this object
            // accountResponse.Accounts.FirstOrDefault().AccountIdentifier
            // accountResponse.Accounts.FirstOrDefault().AccountType
            // accountResponse.Accounts.FirstOrDefault().AvailableBalance
            return accountResponse.Accounts;
        }



        /// <summary>
        /// this redirects to an external URL and passes the account number by default
        /// This is designed to work with Account.cshtmlpage
        /// </summary>
        /// <param name="accountNumber"></param>
        /// <returns></returns>
        public ActionResult RedirectToAccount(string accountNumber)
        {
            // Simple redirect Code start
            // urls should normally be done with a widget setting like below, widget settings are added in the admin screen
            // also note the 
            // var url = GetWidgetSetting("SsoUrl"); //if you want redirect best to use URL
            var url = @"https://www.alkami.com";
            //using System.Collections.Specialized;
            NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
            var userBankIdentifier = this.CurrentUser.BankIdentifier;//Identifier of the customers bank
            var userCustomerId = this.CurrentUser.CustomerId;//customerId is usually a core identifier
            var userIdentifier = this.CurrentUser.UserIdentifier; //alkami value

            // more examples of cureUser object Information
            // var userInfoToPassToSSO = this.CurrentUser.FirstName;
            // var userInfoToPassToSSO = this.CurrentUser.LastName;

            //queryString.Add("useridentifier", userIdentifier.ToString());
            queryString.Add("accountNumber", accountNumber);
            queryString.Add("useridentifier", userIdentifier.ToString());
            queryString.Add("customerId", userCustomerId);
            queryString.Add("userBankIdentifier", userBankIdentifier.ToString());

            var redirectUrl = $"{url}?{queryString.ToString()}";

            // Response.Redirect(redirectUrl, true);
            return Redirect(redirectUrl);
            //end Simple redirect Code 
        }



    }
}
