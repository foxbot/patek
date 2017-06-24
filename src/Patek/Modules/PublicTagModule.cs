using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace Patek.Modules
{
    public class PublicTagModule : TagModuleBase 
    {
        [Command("tags")]
        [Alias("tag list")]
        public async Task ListTagsAsync()
        {
            var tags = Database.Tags.Select(t => t.Name);
            await ReplyAsync("**Tags:** " + string.Join(", ", tags));
        }
    }
}
