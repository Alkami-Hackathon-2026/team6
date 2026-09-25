using HACK26.MS.MyMoneyRules.Data.ProviderSettings;
using HACK26.MS.MyMoneyRules.Service.Integrations;
using System.Threading.Tasks;

namespace HACK26.MS.MyMoneyRules.Service
{
    /// <inheritdoc />
    public partial class ServiceImp
    {
        /// <summary>
        /// Immutable view of the provider settings for a single request
        /// </summary>
        private sealed class ProviderSettingsSnapshot
        {
            public ProviderSettingsSnapshot(string firstSetting, string secondSetting, GeminiOptions gemini)
            {
                FirstSetting = firstSetting;
                SecondSetting = secondSetting;
                Gemini = gemini;
            }

            public string FirstSetting { get; }

            public string SecondSetting { get; }

            public GeminiOptions Gemini { get; }
        }

        /// <summary>
        /// Reads all provider settings in a single scope so each request opens at most one settings scope
        /// </summary>
        private async Task<ProviderSettingsSnapshot> LoadSettingsAsync(Alkami.Contracts.BaseRequest request)
        {
            using (var scope = await GetScopeAsync(request).ConfigureAwait(false))
            {
                return new ProviderSettingsSnapshot(
                    scope.GetSettingOrDefault<string>(SettingNames.FirstProviderSetting),
                    scope.GetSettingOrDefault<string>(SettingNames.SecondProviderSetting),
                    GeminiOptions.From(
                        scope.GetSettingOrDefault<string>(SettingNames.GeminiApiKey),
                        scope.GetSettingOrDefault<string>(SettingNames.GeminiModel)));
            }
        }
    }
}
