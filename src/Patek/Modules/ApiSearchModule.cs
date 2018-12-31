using System;
using System.Threading.Tasks;
using Discord.Commands;
using Patek.Data;
using Patek.Preconditions;
using Patek.Services;

namespace Patek.Modules
{
    public class ApiSearchModule : PatekModuleBase
    {   
        public ApiSearchService ApiSearchService { get; set; }
        
        // I logged this with 900ms response time to the API; factoring in the 100-200ms it takes to reply,
        // we'd be looking at over 1100ms on this command, which is more than I'd like to block for.
        [Command(".net", RunMode = RunMode.Async)]
        [Alias("search", "net", "msftdocs", "c#docs", "netdocs", ".netdocs", "dotnet", "dotnetdocs", "msdn")]
        [Summary("Searches the .NET API Browser for the given search term.")]
        [RateLimit(1, 3, RateLimitType = RateLimitType.Actor)]
        public async Task Search([Remainder] string searchTerm)
        {
            MsdnSearchResults results = null;
            try
            {
                results = await ApiSearchService.GetMsdnResultsAsync(searchTerm);
            }
            catch (ArgumentException)
            {
                await ReplyAsync("The search term was invalid.");
                return;
            }
           
            // non-success error code
            if (results == null)
            {
                await ReplyAsync("Encountered an error trying to get results from MSDN.");
            }
            // no results
            else if (results.Results.Length == 0)
            {
                await ReplyAsync("Search returned no results.");
            }
            // results are not null and contain at least one value
            // so build an embed
            else
            {
                var e = ApiSearchService.BuildResultsEmbed(results, searchTerm);
                await ReplyAsync($"{results.Results.Length} result(s):", embed: e);
            }
        }
    }
}