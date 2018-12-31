using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Discord;
using Discord.Commands;

namespace Patek.Services
{
    public class RateLimitService
    {
        private ConcurrentDictionary<string, RateLimitInfo> _ratelimits;

        public RateLimitService()
        {
            _ratelimits = new ConcurrentDictionary<string, RateLimitInfo>();
        }

        public RateLimitInfo GetOrAdd(string rule, Func<string, RateLimitInfo> createFunc)
            => _ratelimits.GetOrAdd(rule, createFunc);

        /// <summary>
        /// Get a RateLimit rule for global ratelimits.
        /// </summary>
        public string GetRule(CommandInfo command)
            => $"{command.Module.Name}#{command.Name}";
        /// <summary>
        /// Get a RateLimit rule for user ratelimits.
        /// </summary>
        public string GetActorRule(IUser actor, CommandInfo command)
            => $"actor:{actor.Id}/{GetRule(command)}";
        /// <summary>
        /// Get a RateLimit rue for channel ratelimits.
        /// </summary>
        public string GetChanelRule(IChannel channel, CommandInfo command)
            => $"channel:{channel.Id}/{GetRule(command)}";
    }

    public class RateLimitInfo
    {
        public uint Capacity { get; private set; }
        public TimeSpan DrainRate { get; private set; }
        public HashSet<DateTimeOffset> Entries { get; private set; }

        public RateLimitInfo(uint capacity, TimeSpan drainRate)
        {
            Capacity = capacity;
            DrainRate = drainRate;
            Entries = new HashSet<DateTimeOffset>();
        }

        public bool CanEnter()
        {
            int current = 0;
            var now = DateTimeOffset.Now;
            foreach (var entry in Entries)
            {
                if (now - entry > DrainRate)
                    Entries.Remove(entry);
                else
                    current++;
            }
            if (current >= Capacity)
                return false;
            return true;
        }
        
        public void Increment()
        {
            Entries.Add(DateTimeOffset.Now);
        }
    }
}
