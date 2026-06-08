using WebTgBotAssistant.Models;

namespace WebTgBotAssistant.DTO;

public class AddTextMemberReactionDTO
{
    public string Key { get; set; }
    
    public string? StickerId { get; set; }

    public string? Text { get; set; }

    public CompareType CompareType { get; set; }

    public string CompareText { get; set; }
    
    public string Password { get; set; }
}