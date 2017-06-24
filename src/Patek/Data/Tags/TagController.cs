using System;
using Discord;
using Discord.WebSocket;

namespace Patek.Data
{
    public class TagController 
    {
        private readonly DiscordSocketClient _discord;
        private readonly PatekContext _context;
        private readonly Tag _tag;

        public TagController(PatekContext context, DiscordSocketClient discord, Tag tag)
        {
            _context = context;
            _discord = discord;
            _tag = tag;
        }
        
        public void ChangeName(string newName, IUser user)
        {
            _tag.Name = newName;
            LogEvent(AuditType.SetName, user);
        }
        public void ChangeContent(string newContent, IUser user)
        {
            _tag.Content = newContent;
            LogEvent(AuditType.SetContent, user);
        }
        public void ChangeColor(uint color, IUser user)
        {
            _tag.Color = color;
            LogEvent(AuditType.SetColor, user);
        }
        public void Create(IUser user)
            => LogEvent(AuditType.CreatedTag, user);
        public void Use(IUser user)
            => LogEvent(AuditType.UsedTag, user);
        public void Destroy(IUser user)
        {
            _context.Tags.Remove(_tag);
            LogEvent(AuditType.DeletedTag, user);
        }

        public Embed GetEmbed()
        {
            var author = _discord.GetUser(_tag.OwnerId);
            return new EmbedBuilder()
                .WithTitle(_tag.Name)
                .WithDescription(_tag.Content)
                .WithColor(new Color(_tag.Color))
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithName(author?.Username ?? "")
                    .WithIconUrl(author?.GetAvatarUrl() ?? ""))
                .Build();
        }

        private Audit LogEvent(AuditType type, IUser user)
        {
            var audit = new Audit
            {
                AuditType = type,
                TagId = _tag.Id,
                UserId = user.Id,
                Timestamp = DateTimeOffset.Now
            };
            _context.Events.Add(audit);
            return audit;
        }
    }
}
