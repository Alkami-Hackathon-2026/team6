using HACK26.MS.MyMoneyRules.Contracts.Requests;
using HACK26.MS.MyMoneyRules.Contracts.Responses;
using HACK26.MS.MyMoneyRules.Data.Gemini;
using HACK26.MS.MyMoneyRules.Data.ProviderSettings;
using HACK26.MS.MyMoneyRules.Service.Gemini;
using System.Threading.Tasks;

namespace HACK26.MS.MyMoneyRules.Service
{
    public partial class ServiceImp
    {
        public async Task<GeminiStatusResponse> GetGeminiStatusAsync(GetGeminiStatusRequest request)
        {
            var geminiService = await CreateGeminiServiceAsync(request);

            return await Task.FromResult(new GeminiStatusResponse
            {
                Configured = geminiService.IsConfigured,
                Model = geminiService.Model,
                Message = geminiService.IsConfigured
                    ? "Gemini free-tier API is ready for testing."
                    : "Set the Gemini Api Key provider setting or GEMINI_API_KEY in the environment."
            });
        }

        public async Task<GeminiChatResponse> GenerateGeminiChatAsync(GeminiChatRequest request)
        {
            var geminiService = await CreateGeminiServiceAsync(request);
            return await geminiService.GenerateAsync(request);
        }

        private async Task<GeminiService> CreateGeminiServiceAsync(Alkami.Contracts.BaseRequest request)
        {
            string apiKey = null;
            string model = GeminiDefaults.DefaultModel;

            using (var scope = await GetScopeAsync(request))
            {
                apiKey = GeminiService.ResolveApiKey(
                    scope.GetSettingOrDefault<string>(SettingNames.GeminiApiKey));
                model = scope.GetSettingOrDefault<string>(SettingNames.GeminiModel) ?? GeminiDefaults.DefaultModel;
            }

            return new GeminiService(apiKey, model);
        }
    }
}
