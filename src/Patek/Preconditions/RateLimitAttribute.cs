using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using Patek.Services;

namespace Patek.Preconditions
{
    public class RateLimitAttribute : PreconditionAttribute
    {
        public RateLimitType RateLimitType { get; set; }
        private readonly uint _capacity;
        private readonly TimeSpan _drainrate;

        public RateLimitAttribute(uint capacity, int secondsToDrain)
        {
            _capacity = capacity;
            _drainrate = TimeSpan.FromSeconds(secondsToDrain);
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, 
            CommandInfo command, 
            IServiceProvider services)
        {
            var service = services.GetRequiredService<RateLimitService>();
            string rule = GetRule(service, context, command);

            var limit = service.GetOrAdd(rule, Create);
            if (limit.CanEnter())
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError("The rate limit for this command has been exceeded."));
        }

        public string GetRule(RateLimitService service, ICommandContext context, CommandInfo command)
        {
            switch (RateLimitType)
            {
                case RateLimitType.Actor:
                    return service.GetActorRule(context.User, command);
                case RateLimitType.Channel:
                    return service.GetChanelRule(context.Channel, command);
                default:
                    return service.GetRule(command);
            }
        }

        private RateLimitInfo Create(string _)
        {
            return new RateLimitInfo(_capacity, _drainrate);
        }
    }

    public enum RateLimitType
    {
        Global,
        Actor,
        Channel
    }
}
