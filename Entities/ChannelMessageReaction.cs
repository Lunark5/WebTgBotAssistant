using WebTgBotAssistant.Models;

namespace WebTgBotAssistant.Entities;

public class ChannelMessageReaction : MemberReaction
{
    public ChannelMessageReaction() { }
    public ChannelMessageReaction(MemberReaction memberReaction) : base(memberReaction) { }
}