namespace WebTgBotAssistant.Models;

public class AppOptions
{
    public string WebhookUrl { get; set; }
    
    public string TgToken { get; set; }
    
    public string ApiPassword { get; set; }
    
    public OpenAiSettings OpenAiSettings { get; set; }
    
    public string ChannelReplyUserId { get; set; }
    
    public List<string> BotAliases { get; set; }
    
    public List<long> AllowedChatIds { get; set; }
    

    public string[] AdminChatIds { get; set; }
}