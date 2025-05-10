namespace HyperGame.Script.NetworkRequest
{
    using System.Collections.Generic;
    using GameFoundation.Scripts.Network.WebService;
    using GameFoundation.Scripts.Utilities.LogService;
    using GameFoundation.Scripts.Utilities.Utils;
    using Newtonsoft.Json;

    [HttpRequestDefinition("")]
    public class SendGeminiChatRequestHandle : BasePostRequest<GeminiChatResponse>
    {
        private readonly ILogService logger;

        public SendGeminiChatRequestHandle(ILogService logger) : base(logger) { this.logger = logger; }

        public override void Process(GeminiChatResponse responseData)
        {
            // Log the response data
            this.logger.Log($"Chat response data: {JsonConvert.SerializeObject(responseData)}");
        }
    }

    public class GeminiPart
    {
        [JsonProperty("text")] public string Text;
    }

    public class GeminiContent
    {
        [JsonProperty("role")]  public string           Role;
        [JsonProperty("parts")] public List<GeminiPart> Parts;
    }

    public class GeminiRequestData
    {
        [JsonProperty("contents")] public List<GeminiContent> Contents;
    }

    public class GeminiResponseData
    {
        [JsonProperty("content")]      public GeminiContent Contents;
        [JsonProperty("finishReason")] public string        FinishReason;
        [JsonProperty("avgLogprobs")]  public double        AvgLogprobs;
    }

    public class GeminiUsageMetadata
    {
        [JsonProperty("promptTokenCount")]     public int                           PromptTokenCount;
        [JsonProperty("candidatesTokenCount")] public int                           CandidatesTokenCount;
        [JsonProperty("totalTokenCount")]      public int                           TotalTokenCount;
        [JsonProperty("promptTokensDetails")]  public List<GeminiPromptTokenDetail> PromptTokenDetail;

        [JsonProperty("candidatesTokensDetails")]
        public List<GeminiPromptTokenDetail> CandidatesTokenDetail;
    }

    public class GeminiPromptTokenDetail
    {
        [JsonProperty("modality")]   public string Modality;
        [JsonProperty("tokenCount")] public int    TokenCount;
    }

    public class GeminiChatResponse
    {
        [JsonProperty("candidates")]    public List<GeminiResponseData> Candidates;
        [JsonProperty("usageMetadata")] public GeminiUsageMetadata      UsageMetadata;
        [JsonProperty("modelVersion")]  public string                   ModelVersion;
    }
}