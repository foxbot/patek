using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Patek.Data;
using Patek.Preconditions;

namespace Patek.Modules
{
    public class DebugModule : PatekModuleBase
    {
        public CancellationTokenSource ExitTokenSource { get; set; }

        [Command("version")]
        public async Task VersionAsync()
        {
            string git = await Process.Start(
                new ProcessStartInfo("git", "log -n 1 --oneline --abbrev-commit --no-decorate")
                {
                    RedirectStandardOutput = true,
                })
                .StandardOutput.ReadLineAsync();
            string commit = git.Substring(0, 7);
            string commitText = git.Substring(8);

            string dotnet = await Process.Start(
                new ProcessStartInfo("dotnet", "--info")
                {
                    RedirectStandardOutput = true,
                })
                .StandardOutput.ReadToEndAsync();
            string dotnetVersion = dotnet.Split("\n")[1].Substring(12);

            string version = $"{Format.Underline("Appliance Versions")}\n\n" +
                $"- {Format.Bold("Discord.Net:")} {DiscordConfig.Version}\n" +
                $"- {Format.Bold("Patek:")} {Format.Code(commit)} ({commitText})\n" +
                $"- {Format.Bold(".NET Core:")} {dotnetVersion}";
            await ReplyAsync(version);
        }

        [Command("update", RunMode = RunMode.Async)]
        [RequireRole(Role.Owner)]
        public async Task UpdateAsync()
        {
            var git = Process.Start(new ProcessStartInfo("git", "pull -r")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            });
            if (!git.WaitForExit(30000))
                await ReplyAsync("Timed out waiting for pull to complete, something broke. (timeout: 30s)");
            if (git.ExitCode > 0)
            {
                var output = await git.StandardOutput.ReadToEndAsync() + await git.StandardError.ReadToEndAsync();
                if (output.Length > 1900)
                    output = output.Substring(0, 1900) + "...";
                await ReplyAsync($"Pull returned a non-zero exit code. (exit: {git.ExitCode})\n\n```{output}```");
                return;
            }
            await ReactAsync(Ok);
        }

        [Command("shutdown")]
        [RequireRole(Role.Owner)]
        public async Task CloseAsync()
        {
            await ReactAsync(Ok);
            ExitTokenSource.Cancel();
        }
    }
}
