using Alkami.Contracts;
using HACK26.MS.MyMoneyRules.Contracts.Requests;
using HACK26.MS.MyMoneyRules.Contracts.Responses;
using System.ServiceModel;
using System.Threading.Tasks;

namespace HACK26.MS.MyMoneyRules.Contracts
{
    /// <summary>
    /// The IService Contract for HACK26.MS.MyMoneyRules.Contracts
    /// </summary>
    [ServiceContract]
    public interface IMyMoneyRulesServiceContract
    {
        /// <summary>
        /// This is a template method that demonstrates how to get settings from Alkami's data scope abstraction
        /// It is sometimes useful to have a service's settings within a widget or another service.
        /// This service may be one part of a feature, but it's possible to store all feature settings within a single service
        /// Any other widgets or services that are a part of this feature can easily get all the settings they need by calling this method
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        Task<SettingsResponse> GetSettingsAsync(GetSettingsRequest request);

        /// <summary>
        /// Get something from a third party service or API
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        Task<CustomObjectResponse> GetDataAsync(GetSomethingRequest request);

        /// <summary>
        /// Get FDIC Configuration
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        Task<FdicComplianceConfigsResponse> GetFDICConfigurationAsync(GetSomethingRequest request);

        /// <summary>
        /// Returns whether Gemini is configured and which model will be used.
        /// </summary>
        [OperationContract]
        Task<GeminiStatusResponse> GetGeminiStatusAsync(GetGeminiStatusRequest request);

        /// <summary>
        /// Sends a prompt to Gemini and returns the generated text.
        /// </summary>
        [OperationContract]
        Task<GeminiChatResponse> GenerateGeminiChatAsync(GeminiChatRequest request);
    }
}