using Alkami.Contracts;
using Alkami.Data.Validations;
using Common.Logging;
using HACK26.MS.MyMoneyRules.Contracts;
using HACK26.MS.MyMoneyRules.Contracts.Requests;
using HACK26.MS.MyMoneyRules.Contracts.Responses;
using HACK26.MS.MyMoneyRules.Data;
using HACK26.MS.MyMoneyRules.Data.ProviderSettings;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HACK26.MS.MyMoneyRules.Service
{
    /// <inheritdoc />
    public partial class ServiceImp : IMyMoneyRulesServiceContract
    {
        private static readonly ILog Logger = LogManager.GetLogger<ServiceImp>();

        /// <inheritdoc />
        public async Task<SettingsResponse> GetSettingsAsync(GetSettingsRequest request)
        {
            // Create a new response object that encapsulates the data type we'll be returning
            var response = new SettingsResponse();

            // We want some local variables that are available outside of the data scope
            string firstSetting = string.Empty;
            string secondSetting = string.Empty;

            // GetScopeAsync() is how we retrieve settings using the request type of this service
            using (var scope = await GetScopeAsync(request))
            {
                // Assigning the settings to our local variables
                firstSetting = scope.GetSettingOrDefault<string>(SettingNames.FirstProviderSetting);
                secondSetting = scope.GetSettingOrDefault<string>(SettingNames.SecondProviderSetting);
            }

            // It's always good to add a trace log for future troubleshooting
            Logger.Trace($"{nameof(GetSettingsAsync)} | First Setting: [{firstSetting}] | Second Setting: [{secondSetting}]");

            // Populate the details of the Setting object with this service's two template settings
            // The response object's "ItemList" property is an enumerable list of the type we passed into the class definition of SettingsResponse
            response.ItemList.Add(new Setting
            {
                Name = SettingNames.FirstProviderSetting,
                DefaultValue = DefaultSettings()[SettingNames.FirstProviderSetting],
                CurrentValue = firstSetting,
                Description = SettingDescriptors().FirstOrDefault(x => x.Name == SettingNames.FirstProviderSetting)?.Description
            });

            // We'll do the same for the second setting, adding another instance of the Setting to the response's ItemList
            response.ItemList.Add(new Setting
            {
                Name = SettingNames.SecondProviderSetting,
                DefaultValue = DefaultSettings()[SettingNames.SecondProviderSetting],
                CurrentValue = secondSetting,
                Description = SettingDescriptors().FirstOrDefault(x => x.Name == SettingNames.SecondProviderSetting)?.Description
            });

            // Return the response using an awaited task
            return await Task.FromResult(response);
        }

        /// <summary>
        /// We can get something from the web
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<CustomObjectResponse> GetDataAsync(GetSomethingRequest request)
        {
            // A service GET is going to retrieve some data from some other source and populate a response
            // In this case we have a CustomObjectResponse that contains an ItemList of CustomDataObjects
            // For the sake of demonstration a response has been populated with constants and returned to the caller
            return await Task.FromResult(new CustomObjectResponse
            {
                ItemList = new List<CustomDataObject>
                {
                    new CustomDataObject
                    {
                        AnotherPropertyThatsAnInt = 9999,
                        ChildrenObjects = new List<CustomChildObject>
                        {
                            new CustomChildObject
                            {
                                DateTime = DateTime.Now,
                                Int = 007,
                                String = "James Bond"
                            },
                            new CustomChildObject
                            {
                                DateTime = DateTime.Today,
                                Int = 006,
                                String = "Alec Trevelyn"
                            }
                        },
                        OneOfYourObjects = "MI6"
                    }
                }
            });
        }

        ///// <summary>
        ///// https://{bank url}/afx/v2/compliance/fdic-configuration 
        ///// The FDIC Compliance Client Endpoint allows external applications to retrieve the current status of FDIC signage settings for all designated display locations. 
        ///// These include login pages, headers, footers, and inline flows for both desktop and mobile platforms. 
        ///// The endpoint ensures dynamic configuration retrieval, helping maintain compliance with regulatory requirements.
        ///// </summary>
        ///// <returns>FDICComplianceConfigs</returns>
        public async Task<FdicComplianceConfigsResponse> GetFDICConfigurationAsync(GetSomethingRequest request)
        {
            string baseUrl = "https://developer.dev.alkamitech.com/afx/v2/compliance/fdic-configuration";
            FdicComplianceConfigsResponse configurations = new FdicComplianceConfigsResponse();
            // Make the rest request out to the exchange api
            RestClient client = new RestClient(baseUrl);
            var restReq = new RestRequest(Method.GET);
            var response = client.Execute(restReq).Content;

            if (response != null)
            {
                var fdicConfigurations = JsonConvert.DeserializeObject<FdicComplianceConfigs>(response);

                configurations.ItemList.Add(fdicConfigurations);
                return await Task.FromResult(configurations);
            }
            else
            {
                return await Task.FromResult(configurations);
            }
        }
    }
}