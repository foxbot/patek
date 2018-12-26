using System.Collections.Generic;
using System.Linq;

namespace Patek.Data
{
    public static class Role
    {
        static readonly string[] _validRoles;

        static Role()
        {
            _validRoles = typeof(Role)
                .GetFields()
                .Where(f => f.FieldType == typeof(string) && f.IsStatic)
                .Select(f => (string)f.GetValue(null))
                .ToArray();
        }

        public static bool IsValid(string role)
            => _validRoles.Contains(role);
        public static IEnumerable<string> ValidRoles
            => _validRoles;

        public const string Owner = "OWNER";
        public const string AccessSecurity = "ACCESS_SECURITY";
        public const string ChannelBlocks = "BLOCK_CHANNEL";
        public const string ReactionBlocks = "BLOCK_RXN";
    }
}
