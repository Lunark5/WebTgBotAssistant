using System.ClientModel;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using Serilog;
using WebTgBotAssistant.Models;

namespace WebTgBotAssistant;

public class OpenAiClient
{
    private OpenAiSettings _openAiSettings;
    private readonly ChatClient _chatClient;

    public OpenAiClient(IOptions<AppOptions> appOptions)
    {
        _openAiSettings = appOptions.Value.OpenAiSettings;

        if (!_openAiSettings.IsEnabled())
        {
            Log.Information("OpenAI не настроен");

            return;
        }

        var options = new OpenAIClientOptions()
        {
            Endpoint = new Uri(_openAiSettings.Uri)
        };
        var apiKeyCredential = new ApiKeyCredential(_openAiSettings.Token);

        var client = new OpenAIClient(apiKeyCredential, options);

        _chatClient = client.GetChatClient(_openAiSettings.Model);
    }

    public async Task<string> SendMessage(string message)
    {
        if (!_openAiSettings.IsEnabled())
        {
            throw new Exception("OpenAI не настроен");
        }

        if (message.Length > _openAiSettings.MaximumTextLength)
        {
            return _openAiSettings.MaximumTextAnswer;
        }

        var messages = new ChatMessage[]
        {
            new SystemChatMessage(_openAiSettings.Prompt),
            new UserChatMessage(message)
        };

        var completion = await _chatClient.CompleteChatAsync(messages);
        var text = completion.Value.Content[0].Text;

        Log.Information($"OpenAI: {text}");

        return text;
    }
}