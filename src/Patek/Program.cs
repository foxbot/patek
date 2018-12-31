using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LiteDB;
using Nett;
using Patek.Services;

namespace Patek
{
    public class Program : IDisposable
    {
        public static void Main(string[] args)
        {
            using (var program = new Program())
                program.MainAsync().GetAwaiter().GetResult();
        }

        private readonly CancellationTokenSource _exitTokenSource;
        private readonly ServiceProvider _services;

        public Program()
        {
            _exitTokenSource = new CancellationTokenSource();
            _services = BuildServices();

            Console.CancelKeyPress += (_s, e) =>
            {
                e.Cancel = true;
                _exitTokenSource.Cancel();
            };
        }

        public async Task MainAsync()
        {
            _services.GetRequiredService<LogService>().Configure();
            _services.GetRequiredService<ModerationService>().Configure();
            await _services.GetRequiredService<CommandHandlingService>().ConfigureAsync();

            var token = _services.GetRequiredService<Configuration>().Token;
            var discord = _services.GetRequiredService<DiscordSocketClient>();

            await discord.LoginAsync(TokenType.Bot, token);
            await discord.StartAsync();

            Console.Title = $"patek (Discord.Net v{DiscordConfig.Version})";

            try
            {
                await Task.Delay(-1, _exitTokenSource.Token);
            }
            // we expect this to throw when exiting.
            catch (TaskCanceledException) { }

            await discord.StopAsync();
            Environment.Exit(0);
        }

        public ServiceProvider BuildServices()
        {
            return new ServiceCollection()
                .AddSingleton(_ => Toml.ReadFile<Configuration>("./config.toml"))
                .AddSingleton(services =>
                {
                    string db = services.GetRequiredService<Configuration>().Database;
                    return new LiteDatabase(db);
                })
                .AddSingleton(_exitTokenSource)
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<LogService>()
                .AddSingleton<ModerationService>()
                .AddSingleton<ApiSearchService>()
                .AddSingleton<RateLimitService>()
                .BuildServiceProvider();
        }

        public void Dispose()
        {
            _exitTokenSource.Dispose();
            _services.Dispose();
        }
    }
}
