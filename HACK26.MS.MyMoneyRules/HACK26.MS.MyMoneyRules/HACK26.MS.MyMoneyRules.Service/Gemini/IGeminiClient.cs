using HACK26.MS.MyMoneyRules.Contracts.Requests;
using HACK26.MS.MyMoneyRules.Contracts.Responses;
using System.Threading.Tasks;

namespace HACK26.MS.MyMoneyRules.Service.Gemini
{
    /// <summary>
    /// Transport client for the Gemini generateContent API
    /// </summary>
    public interface IGeminiClient
    {
        /// <summary>
        /// Sends the prompt to Gemini using the supplied tenant credentials and model
        /// </summary>
        Task<GeminiChatResponse> GenerateAsync(string apiKey, string model, GeminiChatRequest request);
    }
}
