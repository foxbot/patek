using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using Discord.WebSocket;
using Patek.Services;

namespace Patek
{
    public class RequireElevatedUser : PreconditionAttribute
    {
        private static Task<PreconditionResult> NotUser = Task.FromResult(PreconditionResult.FromError("This command may only be ran in a guild."));
        private static Task<PreconditionResult> NotElevated = Task.FromResult(PreconditionResult.FromError("You are not elevated in this guild."));
        private static Task<PreconditionResult> Elevated = Task.FromResult(PreconditionResult.FromSuccess());

        public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var filter = services.GetRequiredService<FilterService>();
            if (!(context.User is SocketGuildUser user)) return NotUser;
            if (filter.IsElevated(context.User as SocketGuildUser)) return Elevated;
            return NotElevated;
        }
    }
}
