namespace HyperGame.Script.NetworkRequest
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using GameFoundation.Scripts.Network.WebService;
    using GameFoundation.Scripts.Utilities.LogService;
    using GameFoundation.Scripts.Utilities.Utils;
    using Newtonsoft.Json;

    public class SendChatRequest
    {
        public List<MessageData> Messages = new();
        public bool              Stream;
    }

    public class MessageData
    {
        public string Role;
        public string Content;

        public string GetReply()
        {
            // Remove the content between <think> </think> tag and trim the content
            var cleanedContent = Regex.Replace(this.Content, @"<think>.*?</think>", "", RegexOptions.Singleline);

            return cleanedContent.Trim();
        }
    }

    public class SendChatResponse
    {
        [JsonProperty("model")]                public string      Model;
        [JsonProperty("created_at")]           public DateTime    CreatedAt;
        [JsonProperty("message")]              public MessageData Message;
        [JsonProperty("done_reason")]          public string      DoneReason;
        [JsonProperty("done")]                 public bool        Done;
        [JsonProperty("total_duration")]       public double      TotalDuration;
        [JsonProperty("load_duration")]        public double      LoadDuration;
        [JsonProperty("prompt_eval_count")]    public double      PromptEvalCount;
        [JsonProperty("prompt_eval_duration")] public double      PromptEvalDuration;
        [JsonProperty("eval_count")]           public double      EvalCount;
        [JsonProperty("eval_duration")]        public double      EvalDuration;
    }

    [HttpRequestDefinition("chat")]
    public class SendChatHttpDataHandle : BasePostRequest<SendChatResponse>
    {
        private readonly ILogService logger;

        public SendChatHttpDataHandle(ILogService logger) : base(logger) { this.logger = logger; }

        public override void Process(SendChatResponse responseData)
        {
            this.logger.Log($"model: {responseData.Model}");
            this.logger.Log($"message role: {responseData.Message.Role}");
            this.logger.Log($"message content: {responseData.Message.Content}");
        }
    }
}