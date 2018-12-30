using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using LiteDB;
using Patek.Data;

namespace Patek.Services
{
    public class ModerationService : IDisposable
    {
        public static TimeSpan Delay = TimeSpan.FromMinutes(5);

        private readonly CancellationTokenSource _cts;
        private readonly DiscordSocketClient _discord;
        private readonly LiteDatabase _database;
        private readonly Logger _log;

        private Task _task;

        public ModerationService(DiscordSocketClient discord,
            LiteDatabase database, 
            LogService logService)
        {
            _cts = new CancellationTokenSource();
            _discord = discord;
            _database = database;
            _log = new Logger("ModSvc", logService);
        }

        public void Configure()
        {
            _discord.Ready += ConfigureAsync;
        }

        private Task ConfigureAsync()
        {
            if (_task == null)
                _task = RunAsync();

            return Task.CompletedTask;
        }


        public async Task RunAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                var blocks = _database.GetCollection<Block>();
                var now = DateTimeOffset.Now;
                var expired = blocks.Find(b => b.Expiration < now);
                foreach (var expiry in expired)
                {
                    try
                    {
                        await RemoveBlockAsync(expiry);
                    }
                    catch (Exception e)
                    {
                        await _log.WarnAsync("Encountered an error removing an expiry!", e);
                        continue;
                    }
                    blocks.Delete(expiry.Id);
                }
                await Task.Delay(Delay);
            }
        }

        public async Task RemoveBlockAsync(Block block)
        {
            var guild = _discord.GetGuild(block.GuildId);
            var channel = guild.GetChannel(block.ChannelId);
            var target = guild.GetUser(block.TargetId); // must get user from guild, since they won't be able to access channel

            var overwrite = channel.GetPermissionOverwrite(target);
            if (!overwrite.HasValue) return; // must have been manually removed.
            overwrite = overwrite.Value.Modify(viewChannel: PermValue.Inherit);

            var options = RequestOptions.Default;
            options.AuditLogReason = $"Automated block expiry (as requested by actor {block.ActorId})";

            await channel.AddPermissionOverwriteAsync(target, overwrite.Value);
        }
            
        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}
