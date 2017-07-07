using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System.IO;

namespace Patek.Services
{
    public class FilterService
    {
        private readonly Filter _filter;

        public FilterService()
        {
            // TODO: pull filter from database
            _filter = JsonConvert.DeserializeObject<Filter>(File.ReadAllText(Path.Combine(Program.GetConfigRoot(), "filter.json")));
        }
        
        public bool IsWhitelisted(IChannel channel)
            => _filter.ChannelWhitelist.Any(v => channel.Id == v);
        public bool IsElevated(SocketGuildUser user)
        {
            if (!(_filter.GuildRoleMap.TryGetValue(user.Guild.Id, out ulong[] roles))) return false;
            return user.Roles.Any(role => roles.Contains(role.Id));
        }
    }

    public class Filter
    {
        [JsonProperty("whitelist")]
        public ulong[] ChannelWhitelist { get; set; }
        [JsonProperty("guild_role_map")]
        public Dictionary<ulong, ulong[]> GuildRoleMap { get; set; }
    }
}
