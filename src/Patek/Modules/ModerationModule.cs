using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using LiteDB;
using Patek.Data;
using Patek.Preconditions;
using Patek.Services;

namespace Patek.Modules
{
    [RequireBotPermission(ChannelPermission.ManageRoles)]
    [RequireContext(ContextType.Guild)]
    public class ModerationModule : PatekModuleBase
    {
        public LiteDatabase Database { get; set; }
        public ModerationService ModService { get; set; }

        [Command("block")]
        [RequireRole(Role.ChannelBlocks)]
        public Task BlockAsync(IUser target, [Remainder] string reason = null)
            => ModifyPermissionAsync(target, p => p.Modify(viewChannel: PermValue.Deny), reason);

        [Command("unblock")]
        [RequireRole(Role.ChannelBlocks)]
        public Task UnblockAsync(IUser target, [Remainder] string reason = null)
            => ModifyPermissionAsync(target, p => p.Modify(viewChannel: PermValue.Inherit), reason);

        [Command("tempblock")]
        [RequireRole(Role.ChannelBlocks)]
        public async Task TemporaryBlockAsync(IUser target, TimeSpan duration, [Remainder] string reason = null)
        {
            await BlockAsync(target, reason);
            Database.GetCollection<Block>().Insert(new Block
            {
                ActorId = Context.User.Id,
                TargetId = target.Id,
                ChannelId = Context.Channel.Id,
                GuildId = Context.Guild.Id,
                Expiration = DateTimeOffset.Now + duration,
            });
        }

        [Command("react off")]
        [RequireRole(Role.ReactionBlocks)]
        public Task SilenceReactionsAsync(IUser target, [Remainder] string reason = null)
            => ModifyPermissionAsync(target, p => p.Modify(addReactions: PermValue.Deny), reason);

        [Command("react on")]
        [RequireRole(Role.ReactionBlocks)]
        public Task ReinstateReactionsAsync(IUser target, [Remainder] string reason = null)
            =>  ModifyPermissionAsync(target, p => p.Modify(addReactions: PermValue.Inherit), reason);

        private async Task ModifyPermissionAsync(IUser target, Func<OverwritePermissions, OverwritePermissions> permFunc, string reason = null)
        {
            var channel = Context.Channel as SocketTextChannel;
            var overwrite = channel.GetPermissionOverwrite(target) ?? OverwritePermissions.InheritAll;
            overwrite = permFunc(overwrite);

            try
            {
                var options = RequestOptions.Default;
                options.AuditLogReason = $"Action initiated by actor {Context.User.Id}.";
                if (!string.IsNullOrEmpty(reason))
                    options.AuditLogReason += $" ({reason})";

                await (Context.Channel as SocketTextChannel).AddPermissionOverwriteAsync(target, overwrite, options);
            }
            catch (HttpException e) when (e.DiscordCode.GetValueOrDefault() == 50013)
            {
                await ReplyAsync($"Target {target.Id} lives above the bot in hierarchy, unable to act. (error: 50013)");
            }

            await ReactAsync(Ok);
        }
    }
}
