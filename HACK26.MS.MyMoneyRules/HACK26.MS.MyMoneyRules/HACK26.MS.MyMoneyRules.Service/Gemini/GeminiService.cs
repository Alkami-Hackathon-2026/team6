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
    public sealed class GeminiService
    {
        private const string GenerateContentUrlTemplate =
            "https://generativelanguage.googleapis.com/v1beta/models/{0}:generateContent";

        private readonly string _apiKey;
        private readonly string _model;

        public GeminiService(string apiKey, string model)
        {
            _apiKey = apiKey;
            _model = string.IsNullOrWhiteSpace(model) ? GeminiDefaults.DefaultModel : model;
        }

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_apiKey);

        public string Model => _model;

        public async Task<GeminiChatResponse> GenerateAsync(GeminiChatRequest request)
        {
            if (!IsConfigured)
            {
                throw new InvalidOperationException(
                    "Gemini is not configured. Set the Gemini Api Key provider setting or GEMINI_API_KEY in the environment.");
            }

            if (string.IsNullOrWhiteSpace(request?.Prompt))
            {
                throw new ArgumentException("Prompt is required.", nameof(request));
            }

            var restRequest = new RestRequest(Method.POST);
            restRequest.AddQueryParameter("key", _apiKey);

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

            var client = new RestClient(string.Format(GenerateContentUrlTemplate, _model));
            var response = await client.ExecuteAsync(restRequest);

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
                Model = _model,
                Text = text
            };
        }

        public static string ResolveApiKey(string configuredApiKey)
        {
            if (!string.IsNullOrWhiteSpace(configuredApiKey))
            {
                return configuredApiKey;
            }

            return Environment.GetEnvironmentVariable(GeminiDefaults.ApiKeyEnvironmentVariable);
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
