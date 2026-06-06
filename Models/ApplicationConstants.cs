namespace WebTgBotAssistant.Models;

public static class ApplicationConstants
{
    public const string WebhookEndpoint = "/api/webhook";
    public const string AddReactionEndpoint = "/api/addReaction";
    public const string RemoveReactionEndpoint = "/api/removeReaction";

    public const string NewMemberReactionText = "NewMember";
    public const string LeaveMemberReactionText = "LeaveMember";
    public const string ChannelMessageReactionText = "ChannelMessage";
}