namespace WebTgBotAssistant.Models;
using Microsoft.EntityFrameworkCore;

[Index(nameof(Key), IsUnique = true)]
public class MemberReaction
{
    public Guid Id { get; init; }

    public string Key { get; set; }

    public string? ReplyUserId { get; set; }

    public string? StickerId { get; set; }

    public string? Text { get; set; }

    public string? ReplyMarkupText { get; set; }

    public string? ReplyMarkupUri { get; set; }

    public bool IsEmptyReaction() => string.IsNullOrEmpty(StickerId) && string.IsNullOrEmpty(Text);

    public MemberReaction()
    {
        
    }

    public MemberReaction(MemberReaction memberReaction)
    {
        Key = memberReaction.Key;
        StickerId = memberReaction.StickerId;
        Text = memberReaction.Text;
        ReplyMarkupText = memberReaction.ReplyMarkupText;
        ReplyMarkupUri = memberReaction.ReplyMarkupUri;
        ReplyUserId = memberReaction.ReplyUserId;
    }
}