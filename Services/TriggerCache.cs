using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;
using WebTgBotAssistant.Entities;

namespace WebTgBotAssistant.Services;

public class TriggerCache(IServiceProvider serviceProvider)
{
    #region Private

    private ImmutableList<LeaveMemberReaction> _leaveMemberReactions = ImmutableList<LeaveMemberReaction>.Empty;
    private ImmutableList<NewMemberReaction> _newMemberReactions = ImmutableList<NewMemberReaction>.Empty;

    private ImmutableList<ChannelMessageReaction>
        _channelMessageReactions = ImmutableList<ChannelMessageReaction>.Empty;

    private ImmutableList<TextMemberReaction> _textMemberReactions = ImmutableList<TextMemberReaction>.Empty;
    private ImmutableList<TextVariable> _textVariables = ImmutableList<TextVariable>.Empty;

    #endregion

    #region Public

    public IReadOnlyList<LeaveMemberReaction> LeaveMemberReactions => _leaveMemberReactions;

    public IReadOnlyList<NewMemberReaction> NewMemberReactions => _newMemberReactions;
    public IReadOnlyList<ChannelMessageReaction> ChannelMessageReactions => _channelMessageReactions;
    public IReadOnlyList<TextMemberReaction> TextMemberReactions => _textMemberReactions;
    public IReadOnlyList<TextVariable> TextVariables => _textVariables;

    #endregion

    public async Task RefreshAsync()
    {
        using var scope = serviceProvider.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var leaveMemberReactions = await appDbContext.LeaveMemberReactions.AsNoTracking().ToListAsync();
        var newMemberReactions = await appDbContext.NewMemberReactions.AsNoTracking().ToListAsync();
        var channelMessageReactions = await appDbContext.ChannelMessageReactions.AsNoTracking().ToListAsync();
        var textMemberReactions = await appDbContext.TextMemberReactions.AsNoTracking().ToListAsync();
        var textVariables = await appDbContext.TextVariables.AsNoTracking().ToListAsync();

        _leaveMemberReactions = leaveMemberReactions.ToImmutableList();
        _newMemberReactions = newMemberReactions.ToImmutableList();
        _channelMessageReactions = channelMessageReactions.ToImmutableList();
        _textMemberReactions = textMemberReactions.ToImmutableList();
        _textVariables = textVariables.ToImmutableList();
    }
}