using HACK26.MS.MyMoneyRules.Contracts.Requests;
using HACK26.MS.MyMoneyRules.Contracts.Responses;
using HACK26.MS.MyMoneyRules.Data.Gemini;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HACK26.MS.MyMoneyRules.Service.Gemini
{
    /// <inheritdoc />
    public sealed class GeminiClient : IGeminiClient
    {
        private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/";
        private const string GenerateContentResource = "models/{model}:generateContent";
        private const string ApiKeyHeader = "x-goog-api-key";
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

        private readonly IRestClient _restClient;

        /// <summary>
        /// Creates a client with a reusable RestClient and the default timeout
        /// </summary>
        public GeminiClient() : this(new RestClient(BaseUrl) { Timeout = (int)DefaultTimeout.TotalMilliseconds })
        {
        }

        /// <summary>
        /// Creates a client over the supplied RestClient
        /// </summary>
        public GeminiClient(IRestClient restClient)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
        }

        /// <inheritdoc />
        public async Task<GeminiChatResponse> GenerateAsync(string apiKey, string model, GeminiChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "Gemini is not configured. Set the Gemini Api Key provider setting or GEMINI_API_KEY in the environment.");
            }

            if (string.IsNullOrWhiteSpace(request?.Prompt))
            {
                throw new ArgumentException("Prompt is required.", nameof(request));
            }

            model = string.IsNullOrWhiteSpace(model) ? GeminiDefaults.DefaultModel : model;

            var restRequest = new RestRequest(GenerateContentResource, Method.POST);
            restRequest.AddUrlSegment("model", model);
            restRequest.AddHeader(ApiKeyHeader, apiKey);

            var body = new GeminiGenerateContentRequest
            {
                Contents = new List<GeminiContent>
                {
                    new GeminiContent
                    {
                        Parts = new List<GeminiPart>
                        {
                            new GeminiPart { Text = request.Prompt }
                        }
                    }
                }
            };

            if (!string.IsNullOrWhiteSpace(request.SystemInstruction))
            {
                body.SystemInstruction = new GeminiContent
                {
                    Parts = new List<GeminiPart>
                    {
                        new GeminiPart { Text = request.SystemInstruction }
                    }
                };
            }

            restRequest.AddJsonBody(body);

            var response = await _restClient.ExecuteAsync(restRequest);

            if (!response.IsSuccessful)
            {
                var errorMessage = TryReadGeminiError(response.Content) ?? response.ErrorMessage ?? response.StatusDescription;
                throw new InvalidOperationException(
                    string.IsNullOrWhiteSpace(errorMessage)
                        ? "Gemini request failed."
                        : errorMessage);
            }

            var generateContentResponse = JsonConvert.DeserializeObject<GeminiGenerateContentResponse>(response.Content);
            var text = generateContentResponse?.Candidates?
                .FirstOrDefault()?
                .Content?
                .Parts?
                .FirstOrDefault(part => !string.IsNullOrWhiteSpace(part.Text))?
                .Text;

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new InvalidOperationException("Gemini returned an empty response.");
            }

            return new GeminiChatResponse
            {
                Model = model,
                Text = text
            };
        }

        private static string TryReadGeminiError(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            try
            {
                var error = JsonConvert.DeserializeObject<GeminiErrorResponse>(content);
                return error?.Error?.Message;
            }
            catch (JsonException)
            {
                return content;
            }
        }

        private sealed class GeminiGenerateContentRequest
        {
            [JsonProperty("contents")]
            public List<GeminiContent> Contents { get; set; }

            [JsonProperty("systemInstruction")]
            public GeminiContent SystemInstruction { get; set; }
        }

        private sealed class GeminiContent
        {
            [JsonProperty("parts")]
            public List<GeminiPart> Parts { get; set; }
        }

        private sealed class GeminiPart
        {
            [JsonProperty("text")]
            public string Text { get; set; }
        }

        private sealed class GeminiGenerateContentResponse
        {
            [JsonProperty("candidates")]
            public List<GeminiCandidate> Candidates { get; set; }
        }

        private sealed class GeminiCandidate
        {
            [JsonProperty("content")]
            public GeminiContent Content { get; set; }
        }

        private sealed class GeminiErrorResponse
        {
            [JsonProperty("error")]
            public GeminiError Error { get; set; }
        }

        private sealed class GeminiError
        {
            [JsonProperty("message")]
            public string Message { get; set; }
        }
    }
}
