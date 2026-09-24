using Common.Logging;
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
    /// <see cref="IGeminiClient"/> backed by the Gemini generateContent REST API.
    /// Holds a single <see cref="IRestClient"/>; per-request configuration is passed via <see cref="GeminiOptions"/>.
    /// </summary>
    public class GeminiClient : IGeminiClient
    {
        private static readonly ILog Logger = LogManager.GetLogger<GeminiClient>();

        private const string BaseUrl = "https://generativelanguage.googleapis.com";
        private const int DefaultTimeoutMilliseconds = 30000;

        private readonly IRestClient _client;

        /// <summary>
        /// Create a client using a default <see cref="RestClient"/>
        /// </summary>
        public GeminiClient() : this(new RestClient(BaseUrl) { Timeout = DefaultTimeoutMilliseconds })
        {
        }

        /// <summary>
        /// Create a client using the given rest client
        /// </summary>
        public GeminiClient(IRestClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <inheritdoc />
        public async Task<GeminiResult> GenerateAsync(GeminiOptions options, string prompt, string systemInstruction, CancellationToken cancellationToken)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (!options.IsConfigured)
            {
                return GeminiResult.Failure(options.Model, GeminiErrorKind.NotConfigured,
                    "Gemini is not configured. Set the Gemini Api Key provider setting or GEMINI_API_KEY in the environment.");
            }

            if (string.IsNullOrWhiteSpace(prompt))
            {
                return GeminiResult.Failure(options.Model, GeminiErrorKind.InvalidRequest, "A prompt is required.");
            }

            var restRequest = new RestRequest($"v1beta/models/{Uri.EscapeDataString(options.Model)}:generateContent", Method.POST);
            restRequest.AddHeader("x-goog-api-key", options.ApiKey);
            restRequest.AddParameter("application/json", JsonConvert.SerializeObject(BuildBody(prompt, systemInstruction)), ParameterType.RequestBody);

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
                Logger.Error($"{nameof(GeminiClient)} | Transport failure | {options}", ex);
                return GeminiResult.Failure(options.Model, GeminiErrorKind.Unavailable, "Unable to reach the Gemini service.");
            }

            if (restResponse.ResponseStatus != ResponseStatus.Completed)
            {
                Logger.Error($"{nameof(GeminiClient)} | Request did not complete [{restResponse.ResponseStatus}] | {options}", restResponse.ErrorException);
                return restResponse.ResponseStatus == ResponseStatus.TimedOut
                    ? GeminiResult.Failure(options.Model, GeminiErrorKind.Timeout, "The Gemini service timed out.")
                    : GeminiResult.Failure(options.Model, GeminiErrorKind.Unavailable, "Unable to reach the Gemini service.");
            }

            if (!restResponse.IsSuccessful)
            {
                // Raw body is logged for diagnostics only; never surfaced to callers
                Logger.Error($"{nameof(GeminiClient)} | HTTP [{(int)restResponse.StatusCode}] | {options} | {restResponse.Content}");
                return MapStatus(options.Model, restResponse.StatusCode);
            }

            return ParseContent(options.Model, restResponse.Content);
        }

        private static GeminiResult ParseContent(string model, string content)
        {
            try
            {
                var json = JObject.Parse(content ?? string.Empty);
                var text = string.Concat(json.SelectTokens("candidates[0].content.parts[*].text")
                    .Select(t => t.Value<string>())
                    .Where(t => !string.IsNullOrEmpty(t)));

                if (!string.IsNullOrEmpty(text))
                {
                    return GeminiResult.Success(model, text);
                }

                var blockReason = json.SelectToken("promptFeedback.blockReason")?.Value<string>();
                return string.IsNullOrEmpty(blockReason)
                    ? GeminiResult.Failure(model, GeminiErrorKind.InvalidResponse, "Gemini returned an empty response.")
                    : GeminiResult.Failure(model, GeminiErrorKind.Blocked, $"Gemini blocked the prompt [{blockReason}].");
            }
            catch (JsonException ex)
            {
                Logger.Error($"{nameof(GeminiClient)} | Unable to parse response from model [{model}]", ex);
                return GeminiResult.Failure(model, GeminiErrorKind.InvalidResponse, "Gemini returned an unreadable response.");
            }
        }

        private static object BuildBody(string prompt, string systemInstruction)
        {
            var contents = new[] { new { role = "user", parts = new[] { new { text = prompt } } } };

            if (string.IsNullOrWhiteSpace(systemInstruction))
            {
                return new { contents };
            }

            return new
            {
                systemInstruction = new { parts = new[] { new { text = systemInstruction } } },
                contents
            };
        }

        private static GeminiResult MapStatus(string model, HttpStatusCode statusCode)
        {
            var code = (int)statusCode;
            switch (code)
            {
                case 400: return GeminiResult.Failure(model, GeminiErrorKind.InvalidRequest, "Gemini rejected the request.");
                case 401:
                case 403: return GeminiResult.Failure(model, GeminiErrorKind.Unauthorized, "Gemini rejected the configured api key.");
                case 404: return GeminiResult.Failure(model, GeminiErrorKind.ModelNotFound, "The configured Gemini model was not found.");
                case 429: return GeminiResult.Failure(model, GeminiErrorKind.RateLimited, "Gemini rate limit exceeded. Please try again later.");
                default:
                    return code >= 500
                        ? GeminiResult.Failure(model, GeminiErrorKind.Unavailable, "The Gemini service is currently unavailable.")
                        : GeminiResult.Failure(model, GeminiErrorKind.InvalidRequest, $"Gemini request failed with status [{code}].");
            }
        }
    }
}
