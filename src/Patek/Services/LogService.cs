using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Patek.Services
{
    public struct Logger
    {
        private readonly string source;
        private readonly LogService log;

        public Logger(string source, LogService log)
        {
            this.source = source;
            this.log = log;
        }
        
        public Task InfoAsync(string message)
            => log.LogAsync(new LogMessage(LogSeverity.Info, source, message));
        public Task WarnAsync(string message, Exception ex = null)
            => log.LogAsync(new LogMessage(LogSeverity.Warning, source, message, ex));
    }

    public class LogService
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;

        public LogService(CommandService commands, DiscordSocketClient discord)
        {
            _discord = discord;
            _commands = commands;
        }

        public void Configure()
        {
            _commands.Log += LogAsync;
            _discord.Log += LogAsync;
        }

        public Task LogAsync(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }
    }
}
