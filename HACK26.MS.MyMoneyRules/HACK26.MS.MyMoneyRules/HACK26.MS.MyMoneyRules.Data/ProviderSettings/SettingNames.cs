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
        /// Api key used to call the Gemini API
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public const string GeminiApiKey = "Gemini Api Key";

        /// <summary>
        /// Gemini model name used for generation
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public const string GeminiModel = "Gemini Model";
    }
}
