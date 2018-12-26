using System;
using System.Threading.Tasks;
using Discord.Commands;
using Patek.Data;

namespace Patek.Preconditions
{
    public class ValidateRoleAttribute : ParameterPreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
        {
            if (parameter.IsMultiple)
            {
                var input = (string[])value;
                foreach (var role in input)
                    if (!Role.IsValid(role))
                        return Task.FromResult(PreconditionResult.FromError($"'{role}' is not a valid role."));
            }
            else
            {
                var input = (string)value;
                if (!Role.IsValid(input))
                    return Task.FromResult(PreconditionResult.FromError($"'{input}' is not a valid role."));
            }
            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
