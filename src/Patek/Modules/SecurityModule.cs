using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using Discord.Commands;
using LiteDB;
using Patek.Data;
using Patek.Preconditions;

namespace Patek.Modules
{
    [Group("security")]
    public class SecurityModule : PatekModuleBase
    {
        public LiteDatabase Database { get; set; }

        private const string security = "security";

        [Command("inspect")]
        [RequireRole(Role.AccessSecurity)]
        public async Task InspectAsync(IUser target = null)
        {
            target = target ?? Context.User;

            var actor = Database.GetCollection<Actor>(security).FindOne(a => a.Id == target.Id);
            if (actor == null)
            {
                await ReplyAsync($"An actor was not found for that target. (id: {target.Id})");
                return;
            }
            var roles = string.Join("; ", actor.Roles);
            await ReplyAsync($"{Format.Underline($"Actor {target.Id}:")}\n\n{Format.Bold("roles:")} {roles}");
        }

        [Command("grant")]
        [RequireRole(Role.Owner)]
        public async Task GrantAsync(IUser target, [ValidateRole] params string[] roles)
        {
            var actor = Database.GetCollection<Actor>(security).FindOne(a => a.Id == target.Id);
            if (actor == null)
            {
                actor = new Actor
                {
                    Id = target.Id,
                    Roles = new HashSet<string>(),
                };
            }

            int added = 0;
            foreach (var role in roles)
                added += actor.Roles.Add(role) ? 1 : 0;
            
            Database.GetCollection<Actor>(security).Upsert(actor);

            await ReplyAsync($"Actor {actor.Id} was updated. (roles affected: {added})");
        }

        [Command("revoke")]
        [RequireRole(Role.Owner)]
        public async Task RevokeAsync(IUser target, [ValidateRole] params string[] roles)
        {
            var actor = Database.GetCollection<Actor>(security).FindOne(a => a.Id == target.Id);
            if (actor == null)
            {
                await ReplyAsync($"An actor was not found for that target. (id: {target.Id})");
                return;
            }

            int removed = 0;
            int skipped = 0;
            foreach (var role in roles)
            {
                if (actor.Roles.Remove(role))
                    removed += 1;
                else
                    skipped += 1;
            }

            if (removed == 0)
                await ReplyAsync($"No changes were made, actor {actor.Id} possessed none of the expected roles.");
            else
                await ReplyAsync($"Actor {actor.Id} was updated. (roles affected: {removed}, roles skipped: {skipped})");
        }

        [Command("bootstrap")]
        [RequireOwner]
        public async Task BootstrapAsync()
        {
            var target = (await Context.Client.GetApplicationInfoAsync()).Owner;

            var actor = Database.GetCollection<Actor>(security).FindOne(a => a.Id == target.Id);
            if (actor == null)
            {
                actor = new Actor
                {
                    Id = target.Id,
                    Roles = new HashSet<string>(),
                };
            }

            if (actor.Roles.Add(Role.Owner))
                await ReplyAsync($"Actor {actor.Id} was updated. (roles affected: 1)");
            else
                await ReplyAsync($"No changes were made, {actor.Id} already possesses the requested role.");
        }

        [Command("roles")]
        [RequireRole(Role.AccessSecurity)]
        public async Task RolesAsync()
        {
            var roles = string.Join("; ", Role.ValidRoles);
            await ReplyAsync($"Valid roles: {roles}");
        }

        [Command]
        public Task HelpAsync([Remainder] string _)
            => ReplyAsync($"Invalid command. (expected one of: inspect, grant, revoke, bootstrap, roles)");
    }
}
