using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Patek.Data;
using Patek.Preconditions;

namespace Patek.Modules
{
    [RequireBotPermission(ChannelPermission.ManageRoles)]
    [RequireContext(ContextType.Guild)]
    public class ModerationModule : PatekModuleBase
    {
        [Command("block")]
        [RequireRole(Role.ChannelBlocks)]
        public Task BlockAsync(IUser target, string reason = null)
            => ModifyPermissionAsync(target, p => p.Modify(viewChannel: PermValue.Deny), reason);

        [Command("unblock")]
        [RequireRole(Role.ChannelBlocks)]
        public Task UnblockAsync(IUser target, string reason = null)
            => ModifyPermissionAsync(target, p => p.Modify(viewChannel: PermValue.Inherit), reason);

        [Command("tempblock")]
        [RequireRole(Role.ChannelBlocks)]
        public Task TemporaryBlockAsync(IUser target, TimeSpan duration, string reason = null)
        {
            throw new NotImplementedException();
        }

        [Command("react off")]
        [RequireRole(Role.ReactionBlocks)]
        public Task SilenceReactionsAsync(IUser target, string reason = null)
            => ModifyPermissionAsync(target, p => p.Modify(addReactions: PermValue.Deny), reason);

        [Command("react on")]
        [RequireRole(Role.ReactionBlocks)]
        public Task ReinstateReactionsAsync(IUser target, string reason = null)
            =>  ModifyPermissionAsync(target, p => p.Modify(addReactions: PermValue.Inherit), reason);

        private async Task ModifyPermissionAsync(IUser target, Action<OverwritePermissions> permFunc, string reason = null)
        {
            var channel = Context.Channel as SocketTextChannel;
            var overwrite = channel.GetPermissionOverwrite(target) ?? OverwritePermissions.InheritAll;
            permFunc(overwrite);

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
