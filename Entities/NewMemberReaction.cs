using WebTgBotAssistant.Models;

namespace WebTgBotAssistant.Entities;

public class NewMemberReaction : MemberReaction
{
    public NewMemberReaction() { }
    public NewMemberReaction(MemberReaction memberReaction) : base(memberReaction) { }
}