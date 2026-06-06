using WebTgBotAssistant.Models;

namespace WebTgBotAssistant.DTO;

public class AddReactionDTO
{
    public string ReactionType { get; set; }

    public MemberReactionDTO Reaction { get; set; } = null!;
}