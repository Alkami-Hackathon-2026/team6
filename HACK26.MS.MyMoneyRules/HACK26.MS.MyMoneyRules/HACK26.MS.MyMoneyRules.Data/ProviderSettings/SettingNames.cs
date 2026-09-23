using System.Runtime.Serialization;

namespace HACK26.MS.MyMoneyRules.Data.ProviderSettings
{
    /// <summary>
    /// This class contains a list of setting names as string constants
    /// </summary>
    public class SettingNames
    {
        /// <summary>
        /// This is an example provider setting name
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public const string FirstProviderSetting = "First Provider Setting";

        /// <summary>
        /// This is an example provider setting name
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public const string SecondProviderSetting = "Second Provider Setting";

        /// <summary>
        /// Google AI Studio API key for Gemini. Can also be set via GEMINI_API_KEY.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public const string GeminiApiKey = "Gemini Api Key";

        /// <summary>
        /// Gemini model id for the free Developer API tier.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public const string GeminiModel = "Gemini Model";
    }
}
