namespace HyperGame.Script.NetworkRequest.Services
{
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Network.WebService;

    public class ApiHelper
    {
        private readonly IHttpService httpService;

        public ApiHelper(IHttpService httpService) { this.httpService = httpService; }

        public async void SendChatRequest()
        {
            var request = new SendChatRequest
            {
                Messages = new List<MessageData>()
                {
                    new()
                    {
                        Role    = "system",
                        Content = "You are a NPC in the game. Gender female, age 25, occupation student. Japanese anime style. Standing in the middle of the street, looking around."
                    },
                    new()
                    {
                        Role    = "user",
                        Content = "Hello, how are you? Can you help me find the nearest restaurant?"
                    }
                },
                Stream = false
            };
            await this.httpService.SendPostAsync<SendChatHttpDataHandle, SendChatResponse>(request);
        }

        public async UniTask<GeminiChatResponse> SendGeminiChatRequest()
        {
            var request = new GeminiRequestData
            {
                Contents = new List<GeminiContent>()
                {
                    new()
                    {
                        Role = "user",
                        Parts = new List<GeminiPart>()
                        {
                            new() { Text = "Hello, how are you? Can you help me summary the btc trend today?" }
                        }
                    }
                }
            };

            var response = await this.httpService.SendPostAsync<SendGeminiChatRequestHandle, GeminiChatResponse>(request);

            return response;
        }
    }
}