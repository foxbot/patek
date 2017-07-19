using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Patek.Modules
{
    [Group("tag")]
    [RequireElevatedUser]
    public class TagModule : TagModuleBase
    {
        private bool rebuild = false;

        [Command("create")]
        public async Task CreateTagAsync(string name, [Remainder] string content)
        {
            Tags.CreateTag(Database, name, content, Context.User, Color.Default.RawValue);
            await ReactAsync(Pass);
            rebuild = true;
        }
        [Command("set name")]
        public async Task SetNameAsync(string name, string target)
        {
            var tag = Tags.GetTag(Database, name);
            if (tag == null) await ReactAsync(TagNotFound);
            tag.ChangeName(target, Context.User);
            await ReactAsync(Pass);
            rebuild = true;
        }
        [Command("set content")]
        public async Task SetContentAsync(string name, [Remainder] string content)
        {
            var tag = Tags.GetTag(Database, name);
            if (tag == null) await ReactAsync(TagNotFound);
            tag.ChangeContent(content, Context.User);
            await ReactAsync(Pass);
        }
        [Command("set color")]
        public async Task SetColorAsync(string name, uint color)
        {
            var tag = Tags.GetTag(Database, name);
            if (tag == null) await ReactAsync(TagNotFound);
            tag.ChangeColor(color, Context.User);
            await ReactAsync(Pass);
        }
        [Command("destroy")]
        public async Task DeleteTagAsync(string name, bool confirm)
        {
            if (!confirm) await ReactAsync(Fail);
            var tag = Tags.GetTag(Database, name);
            if (tag == null) await ReactAsync(TagNotFound);
            tag.Destroy(Context.User);
            await ReactAsync(Removed);
            rebuild = true;
        }
        [Command("rebuild")]
        public async Task RebuildTags()
        {
            await Tags.BuildCommandsAsync();
            await ReactAsync(Pass);
        }

        private Task ReactAsync(Emoji emoji)
            => Context.Message.AddReactionAsync(emoji);
        protected override void AfterExecute(CommandInfo context)
        {
            Database.SaveChanges();
            Database.Dispose();
            // rebuild commands after commiting changes to database!
            if (rebuild)
                Tags.BuildCommandsAsync().GetAwaiter().GetResult();
        }
   }
}
