using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Patek.Services
{
    public class CommandHandlingService
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly LogService _log;
        private readonly IServiceProvider _services;

        public CommandHandlingService(CommandService commands,
            DiscordSocketClient discord,
            LogService log,
            IServiceProvider services)
        {
            _commands = commands;
            _discord = discord;
            _log = log;
            _services = services;

            _commands.CommandExecuted += OnCommandExecutedAsync;
            _discord.MessageReceived += OnMessageReceivedAsync;
        }

        public async Task ConfigureAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }
        
        public async Task OnMessageReceivedAsync(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage msg)) return;
            if (msg.Author.IsBot) return;

            int argPos = 0;
            if (!(msg.HasMentionPrefix(_discord.CurrentUser, ref argPos) 
                || msg.HasCharPrefix('/', ref argPos))) return;

            var context = new SocketCommandContext(_discord, msg);
            await _commands.ExecuteAsync(context, argPos, _services);
        }

        public async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified) return; // ignore search failures
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ToString());
            var log = new LogMessage(LogSeverity.Info, "chs", $"{context.User} invoked {command.Value.Name} in {context.Channel} with {result}");
            await _log.LogAsync(log);
        }
    }
}
