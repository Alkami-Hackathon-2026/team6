using Common.Logging;
using HACK26.MS.MyMoneyRules.Contracts.Requests;
using HACK26.MS.MyMoneyRules.Contracts.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace HACK26.MS.MyMoneyRules.Service.Integrations
{
    /// <summary>
    /// Default values used by <see cref="GeminiService"/>
    /// </summary>
    public static class GeminiDefaults
    {
        /// <summary>Default Gemini model (free tier)</summary>
        public const string DefaultModel = "gemini-3.6-flash";

        /// <summary>Gemini REST API base url</summary>
        public const string BaseUrl = "https://generativelanguage.googleapis.com";

        /// <summary>Environment variable used when the provider setting is not configured</summary>
        public const string ApiKeyEnvironmentVariable = "GEMINI_API_KEY";

        /// <summary>Request timeout in milliseconds</summary>
        public const int TimeoutMilliseconds = 30000;
    }

    /// <summary>
    /// Thin client over the Gemini generateContent REST API
    /// </summary>
    public class GeminiService
    {
        private static readonly ILog Logger = LogManager.GetLogger<GeminiService>();

        private readonly string _apiKey;
        private readonly IRestClient _client;

        /// <summary>
        /// Create a Gemini client
        /// </summary>
        public GeminiService(string apiKey, string model)
            : this(apiKey, model, new RestClient(GeminiDefaults.BaseUrl) { Timeout = GeminiDefaults.TimeoutMilliseconds })
        {
        }

        /// <summary>
        /// Create a Gemini client using the given rest client
        /// </summary>
        public GeminiService(string apiKey, string model, IRestClient client)
        {
            _apiKey = apiKey;
            _client = client ?? throw new ArgumentNullException(nameof(client));
            Model = string.IsNullOrWhiteSpace(model) ? GeminiDefaults.DefaultModel : model.Trim();
        }

        /// <summary>The model used for generation</summary>
        public string Model { get; }

        /// <summary>True when an api key is available</summary>
        public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

        /// <summary>
        /// Resolve the api key from the provider setting, falling back to the environment
        /// </summary>
        public static string ResolveApiKey(string settingValue)
        {
            if (!string.IsNullOrWhiteSpace(settingValue))
            {
                return settingValue.Trim();
            }

            var environmentValue = Environment.GetEnvironmentVariable(GeminiDefaults.ApiKeyEnvironmentVariable);
            return string.IsNullOrWhiteSpace(environmentValue) ? null : environmentValue.Trim();
        }

        /// <summary>
        /// Generate a completion for the given prompt. Failures are reported on the response rather than thrown.
        /// </summary>
        public async Task<GeminiChatResponse> GenerateAsync(GeminiChatRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = new GeminiChatResponse { Model = Model };

            if (!IsConfigured)
            {
                return Fail(response, "Gemini is not configured. Set the Gemini Api Key provider setting or GEMINI_API_KEY in the environment.");
            }

            if (string.IsNullOrWhiteSpace(request?.Prompt))
            {
                return Fail(response, "A prompt is required.");
            }

            var restRequest = new RestRequest($"v1beta/models/{Model}:generateContent", Method.POST);
            restRequest.AddHeader("x-goog-api-key", _apiKey);
            restRequest.AddParameter("application/json", JsonConvert.SerializeObject(BuildBody(request)), ParameterType.RequestBody);

            IRestResponse restResponse;
            try
            {
                restResponse = await _client.ExecuteAsync(restRequest, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.Error($"{nameof(GeminiService)} | Transport failure calling model [{Model}]", ex);
                return Fail(response, "Unable to reach the Gemini service.");
            }

            if (restResponse.ResponseStatus != ResponseStatus.Completed)
            {
                Logger.Error($"{nameof(GeminiService)} | Request did not complete [{restResponse.ResponseStatus}] | {restResponse.ErrorMessage}", restResponse.ErrorException);
                return Fail(response, restResponse.ResponseStatus == ResponseStatus.TimedOut
                    ? "The Gemini service timed out."
                    : "Unable to reach the Gemini service.");
            }

            if (!restResponse.IsSuccessful)
            {
                // Never surface the raw body to callers; it may echo request details
                Logger.Error($"{nameof(GeminiService)} | HTTP [{(int)restResponse.StatusCode}] from model [{Model}] | {restResponse.Content}");
                return Fail(response, DescribeStatus(restResponse.StatusCode));
            }

            try
            {
                var json = JObject.Parse(restResponse.Content ?? string.Empty);
                var text = json.SelectTokens("candidates[0].content.parts[*].text")
                    .Select(t => t.Value<string>())
                    .Where(t => !string.IsNullOrEmpty(t));

                response.Text = string.Concat(text);

                if (string.IsNullOrEmpty(response.Text))
                {
                    var blockReason = json.SelectToken("promptFeedback.blockReason")?.Value<string>();
                    return Fail(response, string.IsNullOrEmpty(blockReason)
                        ? "Gemini returned an empty response."
                        : $"Gemini blocked the prompt [{blockReason}].");
                }
            }
            catch (JsonException ex)
            {
                Logger.Error($"{nameof(GeminiService)} | Unable to parse response from model [{Model}]", ex);
                return Fail(response, "Gemini returned an unreadable response.");
            }

            return response;
        }

        private static object BuildBody(GeminiChatRequest request)
        {
            var contents = new[] { new { role = "user", parts = new[] { new { text = request.Prompt } } } };

            if (string.IsNullOrWhiteSpace(request.SystemInstruction))
            {
                return new { contents };
            }

            return new
            {
                systemInstruction = new { parts = new[] { new { text = request.SystemInstruction } } },
                contents
            };
        }

        private static string DescribeStatus(HttpStatusCode statusCode)
        {
            switch ((int)statusCode)
            {
                case 400: return "Gemini rejected the request.";
                case 401:
                case 403: return "Gemini rejected the configured api key.";
                case 404: return "The configured Gemini model was not found.";
                case 429: return "Gemini rate limit exceeded. Please try again later.";
                default: return (int)statusCode >= 500
                    ? "The Gemini service is currently unavailable."
                    : $"Gemini request failed with status [{(int)statusCode}].";
            }
        }

        private static GeminiChatResponse Fail(GeminiChatResponse response, string message)
        {
            response.HasError = true;
            response.SystemMessage = message;
            return response;
        }
    }
}
