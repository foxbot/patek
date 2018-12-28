using System;

namespace Patek.Data
{
    // Container used to hold temp-block expiries
    public class Block
    {
        public int Id { get; set; }
        public ulong ActorId { get; set; }
        public ulong TargetId { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public DateTimeOffset Expiration { get; set; }
    }
}
