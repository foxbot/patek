using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using LiteDB;
using Patek.Data;

namespace Patek.Preconditions
{
    public class RequireRoleAttribute : PreconditionAttribute
    {
        private readonly string _role;

        public RequireRoleAttribute(string role)
        {
            if (!Role.IsValid(role))
                throw new ArgumentException($"The required role {role} is not valid!", nameof(role));
            _role = role;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var database = services.GetRequiredService<LiteDatabase>();
            var actor = database.GetCollection<Actor>("security").FindOne(a => a.Id == context.User.Id);
            if (actor == null)
                return Task.FromResult(PreconditionResult.FromError("Sorry, you lack the required permissions for this command."));
            if (!(actor.Roles.Contains(_role) || actor.Roles.Contains(Role.Owner)))
                return Task.FromResult(PreconditionResult.FromError($"Sorry, your actor lacks the required permissions for this command. (needs: {_role}"));
            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
