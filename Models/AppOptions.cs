namespace WebTgBotAssistant.Models;

public class AppOptions
{
    public string WebhookUrl { get; set; }
    public string TgToken { get; set; }
    
    public string ChannelReplyUserId { get; set; }
    public List<long> AllowedChatIds { get; set; }
    
    public string[] AdminChatIds { get; set; }
}