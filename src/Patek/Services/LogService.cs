using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Patek.Services
{
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
