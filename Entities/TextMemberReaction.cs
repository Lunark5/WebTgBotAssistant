using WebTgBotAssistant.Models;

namespace WebTgBotAssistant.Entities;

public class TextMemberReaction : MemberReaction
{
    public CompareType CompareType { get; set; }

    public string CompareText { get; set; }
}