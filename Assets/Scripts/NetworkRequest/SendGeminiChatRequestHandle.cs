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

    public class GeminiChatResponse
    {
        [JsonProperty("candidates")] public List<GeminiRequestData> Candidates;
    }
}