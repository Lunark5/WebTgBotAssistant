using WebTgBotAssistant.Models;

namespace WebTgBotAssistant.DTO;

public class RemoveReactionDTO
{
    public MemberReactionType ReactionType { get; set; }

    public string Key { get; set; }
    
    public string Password { get; set; }
}