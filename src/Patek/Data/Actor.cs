using System.Collections.Generic;

namespace Patek.Data
{
    public class Actor
    {
        public ulong Id { get; set; }
        public HashSet<string> Roles { get; set; }
    }
}
