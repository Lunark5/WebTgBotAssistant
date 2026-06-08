using WebTgBotAssistant.Models;

namespace WebTgBotAssistant.DTO;

public class AddReactionDTO
{
    public MemberReactionType ReactionType { get; set; }

    public MemberReactionDTO Reaction { get; set; } = null!;

    public string Password { get; set; }
}