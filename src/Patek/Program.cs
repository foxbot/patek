using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Patek.Services;
using Patek.Data;
using System.Linq;

namespace Patek
{
    class Program
    {
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private IConfiguration _config;

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _config = BuildConfig();

            var services = ConfigureServices();
            services.GetRequiredService<LogService>();
            await services.GetRequiredService<CommandHandlingService>().InitializeAsync(services);
            await services.GetRequiredService<TagService>().InitializeAsync(services);

            await _client.LoginAsync(TokenType.Bot, _config["token"]);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                // Base
                .AddSingleton(_client)
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                // Logging
                .AddLogging()
                .AddSingleton<LogService>()
                // Extra
                .AddSingleton(_config)
                .AddDbContext<PatekContext>(options =>
                {
                    options.UseNpgsql(_config["db"]);
                }, ServiceLifetime.Transient)
                .AddSingleton<TagService>()
                .AddSingleton<FilterService>()
                .BuildServiceProvider();
        }

        private IConfiguration BuildConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(GetConfigRoot())
                .AddJsonFile("config.json")
                .Build();
        }

        public static string GetConfigRoot()
        {
            // Get whether the app is being launched from / (deployed) or /src/Patek (debug)

            var cwd = Directory.GetCurrentDirectory();
            var sln = Directory.GetFiles(cwd).Any(f => f.Contains("Patek.sln"));
            return sln ? cwd : Path.Combine(cwd, "../..");
        }
    }
}