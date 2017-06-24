using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.EmojiTools;
using Discord.Commands;
using Patek.Data;
using Patek.Services;

namespace Patek.Modules
{
    public class TagModule : ModuleBase<SocketCommandContext>
    {
        static readonly Emoji TagNotFound = EmojiExtensions.FromText("mag_right");
        static readonly Emoji Pass = EmojiExtensions.FromText("ok_hand");
        static readonly Emoji Fail = EmojiExtensions.FromText("octagonal_sign");
        static readonly Emoji Removed = EmojiExtensions.FromText("put_litter_in_its_place");

        public PatekContext Database { get; set; } 
        public TagService Tags { get; set; }

        [Command("tags")]
        [Alias("tag list")]
        public async Task ListTagsAsync()
        {
            var tags = Database.Tags.Select(t => t.Name);
            await ReplyAsync("**Tags:** " + string.Join(", ", tags));
        }
        [Command("tag create")]
        public async Task CreateTagAsync(string name, [Remainder] string content)
        {
            Tags.CreateTag(Database, name, content, Context.User, Color.Default.RawValue);
            await ReactAsync(Pass);
            await Tags.BuildCommandsAsync();
        }
        [Command("tag set name")]
        public async Task SetNameAsync(string name, string target)
        {
            var tag = Tags.GetTag(Database, name);
            if (tag == null) await ReactAsync(TagNotFound);
            tag.ChangeName(target, Context.User);
            await ReactAsync(Pass);
        }
        [Command("tag set content")]
        public async Task SetContentAsync(string name, [Remainder] string content)
        {
            var tag = Tags.GetTag(Database, name);
            if (tag == null) await ReactAsync(TagNotFound);
            tag.ChangeContent(content, Context.User);
            await ReactAsync(Pass);
        }
        [Command("tag set color")]
        public async Task SetColorAsync(string name, uint color)
        {
            var tag = Tags.GetTag(Database, name);
            if (tag == null) await ReactAsync(TagNotFound);
            tag.ChangeColor(color, Context.User);
            await ReactAsync(Pass);
        }
        [Command("tag destroy")]
        public async Task DeleteTagAsync(string name, bool confirm)
        {
            if (!confirm) await ReactAsync(Fail);
            var tag = Tags.GetTag(Database, name);
            if (tag == null) await ReactAsync(TagNotFound);
            tag.Destroy(Context.User);
            await ReactAsync(Removed);
            await Tags.BuildCommandsAsync();
        }
        [Command("tag rebuild")]
        public async Task RebuildTags()
        {
            await Tags.BuildCommandsAsync();
            await ReactAsync(Pass);
        }

        private Task ReactAsync(Emoji emoji)
            => Context.Message.AddReactionAsync(emoji);
        protected override void AfterExecute()
        {
            Database.SaveChanges();
            Database.Dispose(); 
        }
   }
}
