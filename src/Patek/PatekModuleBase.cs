using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Patek
{
    public class PatekModuleBase : ModuleBase<SocketCommandContext>
    {
        public IEmote Ok => new Emoji("🆗");

        public Task ReactAsync(IEmote emote)
            => Context.Message.AddReactionAsync(emote);
    }
}