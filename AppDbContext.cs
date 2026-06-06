using Microsoft.EntityFrameworkCore;
using WebTgBotAssistant.Entities;
using WebTgBotAssistant.Models;

namespace WebTgBotAssistant;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<LeaveMemberReaction> LeaveMemberReactions => Set<LeaveMemberReaction>();
    public DbSet<NewMemberReaction> NewMemberReactions => Set<NewMemberReaction>();
    public DbSet<ChannelMessageReaction> ChannelMessageReactions => Set<ChannelMessageReaction>();

    public async Task AddAsync(string memberReactionType, MemberReaction memberReaction)
    {
        switch (memberReactionType)
        {
            case ApplicationConstants.LeaveMemberReactionText:
            {
                await LeaveMemberReactions.AddAsync(new LeaveMemberReaction(memberReaction));

                break;
            }
            case ApplicationConstants.NewMemberReactionText:
            {
                await NewMemberReactions.AddAsync(new NewMemberReaction(memberReaction));

                break;
            }
            case ApplicationConstants.ChannelMessageReactionText:
            {
                await ChannelMessageReactions.AddAsync(new ChannelMessageReaction(memberReaction));

                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(memberReactionType), memberReactionType, null);
        }

        await SaveChangesAsync();
    }

    public async Task RemoveAsync(string memberReactionType, string key)
    {
        switch (memberReactionType)
        {
            case ApplicationConstants.LeaveMemberReactionText:
            {
                var entity = LeaveMemberReactions.FirstOrDefault(x => x.Key == key);
                if (entity != null)
                {
                    LeaveMemberReactions.Remove(entity);
                }

                break;
            }
            case ApplicationConstants.NewMemberReactionText:
            {
                var entity = NewMemberReactions.FirstOrDefault(x => x.Key == key);
                if (entity != null)
                {
                    NewMemberReactions.Remove(entity);
                }

                break;
            }
            case ApplicationConstants.ChannelMessageReactionText:
            {
                var entity = ChannelMessageReactions.FirstOrDefault(x => x.Key == key);
                if (entity != null)
                {
                    ChannelMessageReactions.Remove(entity);
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(memberReactionType), memberReactionType, null);
        }

        await SaveChangesAsync();
    }

    public bool IsExists(string memberReactionType, string key)
    {
        switch (memberReactionType)
        {
            case ApplicationConstants.LeaveMemberReactionText:
            {
                var entity = LeaveMemberReactions.FirstOrDefault(x => x.Key == key);
                if (entity != null)
                {
                    return true;
                }

                break;
            }
            case ApplicationConstants.NewMemberReactionText:
            {
                var entity = NewMemberReactions.FirstOrDefault(x => x.Key == key);
                if (entity != null)
                {
                    return true;
                }

                break;
            }
            case ApplicationConstants.ChannelMessageReactionText:
            {
                var entity = ChannelMessageReactions.FirstOrDefault(x => x.Key == key);
                if (entity != null)
                {
                    return true;
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(memberReactionType), memberReactionType, null);
        }

        return false;
    }
}