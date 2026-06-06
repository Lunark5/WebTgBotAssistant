using WebTgBotAssistant.Models;

namespace WebTgBotAssistant.Entities;

public class LeaveMemberReaction : MemberReaction
{
    public LeaveMemberReaction() { }
    public LeaveMemberReaction(MemberReaction memberReaction) : base(memberReaction) { }
}