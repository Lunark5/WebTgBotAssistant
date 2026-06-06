namespace WebTgBotAssistant.DTO;

public class MemberReactionDTO
{
    public string Key { get; set; }
    
    public string? StickerId { get; set; }

    public string? Text { get; set; }

    public string? ReplyMarkupText { get; set; }

    public string? ReplyMarkupUri { get; set; }
}