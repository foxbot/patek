using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Patek.Data;
using Patek.Services;

namespace Patek.Modules
{
    public class ApiSearchModule : PatekModuleBase
    {   
        public ApiSearchService ApiSearchService { get; set; }
        
        [Command(".net")]
        [Alias("search", "net", "msftdocs", "c#docs", "netdocs", ".netdocs", "dotnet", "dotnetdocs")]
        [Summary("Searches the .NET API Browser for the given search term.")]
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
            else if (results.Results.Count == 0)
            {
                await ReplyAsync("Search returned no results.");
            }
            // results are not null and contain at least one value
            // so build an embed
            else
            {
                var e = ApiSearchService.BuildResultsEmbed(results, searchTerm);
                await ReplyAsync($"{results.Results.Count} result(s):", embed: e);
            }
        }
    }
}