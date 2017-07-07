using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Patek.Data
{
    // Migrations
    public class PatekContextFactory : IDbContextFactory<PatekContext>
    {
        public PatekContext Create(DbContextFactoryOptions options)
        {
            var config = new ConfigurationBuilder()
                // config -> /config.json
                // cwd -> /src/Patek.Data/
                .SetBasePath(Path.Combine(options.ContentRootPath, "../.."))
                .AddJsonFile("config.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<PatekContext>()
                .UseNpgsql(config["db"]);
            return new PatekContext(optionsBuilder.Options);
        }
    }
}
