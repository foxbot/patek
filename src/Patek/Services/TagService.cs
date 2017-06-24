using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using Discord.WebSocket;
using Discord;
using Patek.Data;

namespace Patek.Services
{
    public class TagService
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private IServiceProvider _services;
        private ModuleInfo module;

        public TagService(CommandService commands, DiscordSocketClient discord)
        {
            _commands = commands;
            _discord = discord;
        }
        public async Task InitializeAsync(IServiceProvider services)
        {
            _services = services;
            await BuildCommandsAsync();
        }

        public TagController GetTag(PatekContext context, string name)
        {
            var tag = context.Tags
                .SingleOrDefault(t => t.Name == name);
            return new TagController(context, _discord, tag);
        }
        public TagController CreateTag(PatekContext context, string name, string content, IUser author, uint color)
        {
            var tag = new Tag
            {
                Name = name,
                Content = content,
                OwnerId = author.Id,
                Color = color
            };
            context.Tags.Add(tag);
            var controller = new TagController(context, _discord, tag);
            controller.Create(author);
            return controller;
        }
        
        public async Task BuildCommandsAsync()
        {
            if (module != null)
                await _commands.RemoveModuleAsync(module);

            using (var context = _services.GetService<PatekContext>())
            {
                var tags = context.Tags.AsNoTracking();

                module = await _commands.CreateModuleAsync("", m =>
                {
                    foreach (var tag in tags)
                    {
                        m.AddCommand(tag.Name, async (ctx, _, provider) =>
                        {
                            using (var db = provider.GetService<PatekContext>())
                            {
                                var controller = GetTag(db, tag.Name);
                                var embed = controller.GetEmbed();

                                await ctx.Channel.SendMessageAsync("", embed: embed);

                                controller.Use(ctx.User);
                                await db.SaveChangesAsync();
                            }
                        }, command => { });
                    }
                });
            }
        }
    }
}
