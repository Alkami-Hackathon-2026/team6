using System;

namespace HACK26.MS.MyMoneyRules.Service.Integrations
{
    /// <summary>
    /// Immutable, per-request Gemini configuration resolved from provider settings
    /// </summary>
    public sealed class GeminiOptions
    {
        /// <summary>Default Gemini model (free tier)</summary>
        public const string DefaultModel = "gemini-2.0-flash";

        /// <summary>Environment variable used when the provider setting is not configured</summary>
        public const string ApiKeyEnvironmentVariable = "GEMINI_API_KEY";

        private GeminiOptions(string apiKey, string model)
        {
            ApiKey = apiKey;
            Model = model;
        }

        /// <summary>Api key; null when not configured</summary>
        public string ApiKey { get; }

        /// <summary>Model used for generation</summary>
        public string Model { get; }

        /// <summary>True when an api key is available</summary>
        public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);

        /// <summary>
        /// Build options from raw setting values, falling back to the environment and defaults
        /// </summary>
        public static GeminiOptions From(string apiKeySetting, string modelSetting)
        {
            var apiKey = !string.IsNullOrWhiteSpace(apiKeySetting)
                ? apiKeySetting
                : Environment.GetEnvironmentVariable(ApiKeyEnvironmentVariable);

            return new GeminiOptions(
                string.IsNullOrWhiteSpace(apiKey) ? null : apiKey.Trim(),
                string.IsNullOrWhiteSpace(modelSetting) ? DefaultModel : modelSetting.Trim());
        }

        /// <inheritdoc />
        public override string ToString() => $"Model [{Model}] | Configured [{IsConfigured}]";
    }

    /// <summary>
    /// Categorized failure reasons so callers can react without parsing messages
    /// </summary>
    public enum GeminiErrorKind
    {
        /// <summary>No error</summary>
        None,
        /// <summary>No api key available</summary>
        NotConfigured,
        /// <summary>Caller input was invalid</summary>
        InvalidRequest,
        /// <summary>Api key rejected</summary>
        Unauthorized,
        /// <summary>Model not found</summary>
        ModelNotFound,
        /// <summary>Rate limited</summary>
        RateLimited,
        /// <summary>Timed out</summary>
        Timeout,
        /// <summary>Network or 5xx failure</summary>
        Unavailable,
        /// <summary>Prompt blocked by safety filters</summary>
        Blocked,
        /// <summary>Unparseable or empty response</summary>
        InvalidResponse
    }

    /// <summary>
    /// Outcome of a Gemini generation call
    /// </summary>
    public sealed class GeminiResult
    {
        private GeminiResult(string model, string text, GeminiErrorKind errorKind, string errorMessage)
        {
            Model = model;
            Text = text;
            ErrorKind = errorKind;
            ErrorMessage = errorMessage;
        }

        /// <summary>Model that handled the request</summary>
        public string Model { get; }

        /// <summary>Generated text</summary>
        public string Text { get; }

        /// <summary>Failure category</summary>
        public GeminiErrorKind ErrorKind { get; }

        /// <summary>Caller-safe error message</summary>
        public string ErrorMessage { get; }

        /// <summary>True when generation succeeded</summary>
        public bool IsSuccess => ErrorKind == GeminiErrorKind.None;

        /// <summary>Create a successful result</summary>
        public static GeminiResult Success(string model, string text) => new GeminiResult(model, text, GeminiErrorKind.None, null);

        /// <summary>Create a failed result</summary>
        public static GeminiResult Failure(string model, GeminiErrorKind kind, string message) => new GeminiResult(model, null, kind, message);
    }
}
