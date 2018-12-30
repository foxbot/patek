using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Patek.Data;

namespace Patek.Modules
{
    public class DotNetApiSearchModule : PatekModuleBase
    {
        /// <summary>
        ///     Regular expression that determines what search queries are valid.
        ///     This is stated in error messages when invalid terms are searched.
        /// </summary>
        private const string SearchTermRegex = @"^[A-Za-z][A-Za-z0-9\\.<>,]+$";
        
        /// <summary>
        ///     Up to the first N results to display in the embed.
        /// </summary>
        private const int NumResultsToDisplay = 3;
        
        [Command(".net")]
        [Alias("search", "net", "msftdocs", "c#docs", "netdocs", ".netdocs", "dotnet", "dotnetdocs")]
        [Summary("Searches the .NET API Browser for the given search term.")]
        public async Task Search([Remainder] string searchTerm)
        {
            SearchResults results = null;
            try
            {
                results = await GetMsdnResultsAsync(searchTerm);
            }
            catch (ArgumentException e)
            {
                // is it bad practice to return exception messages?
                await ReplyAsync(e.Message);
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
                var e = BuildResultsEmbed(results, searchTerm);
                await ReplyAsync($"First {Math.Min(results.Results.Count, NumResultsToDisplay)} results:", embed: e);
            }
        }

        /// <summary>
        ///     Builds an Embed for the given SearchResults.
        /// </summary>
        /// <param name="results"> The search results to display in the embed. Assumed not null. </param>
        /// <param name="searchTerm"> The search term that produced these results. Assumed not null. </param>
        /// <returns> A new Embed instance containing the search results information. </returns>
        private static Embed BuildResultsEmbed(SearchResults results, string searchTerm)
        {
            var eb = new EmbedBuilder();
            eb.WithCurrentTimestamp();
            eb.WithTitle($".NET API Browser Search Results: \"{searchTerm}\"");
            // .NET purple
            eb.WithColor(new Color(104, 33, 122));
            
            // use the embed description to show all results
            // making the assumption that I don't need to worry about the embed description field
            // limit
            var sb = new StringBuilder();
            foreach (var x in results.Results.Take(NumResultsToDisplay))
            {
                sb.AppendLine($"{x.ItemKind} [{x.DisplayName}]({x.Url})\n{x.Description}");
            }
            // include a link to the website itself that includes all results
            sb.AppendLine($"[Find more results in the .NET API Browser]{GetMsdnFrontEndSearch(searchTerm)}");
            eb.WithDescription(sb.ToString());

            return eb.Build();
        }
        
        /// <summary>
        ///     Gets the query string of the docs.microsoft.com api browser with the search parameter populated.
        /// </summary>
        /// <param name="searchTerm"> The term to search for. Assumes not null or whitespace. </param>
        /// <returns> The query string with the url encoded search term. </returns>
        private static string GetMsdnApiSearchUrl(string searchTerm)
            => $"https://docs.microsoft.com/api/apibrowser/dotnet/search?search={WebUtility.UrlEncode(searchTerm)}";

        /// <summary>
        ///     Gets the query string of the front-end url for the given search parameter.
        ///     This is what users will see in their web browser.
        /// </summary>
        /// <param name="searchTerm"> The term to search for. Assumes not null or whitespace. </param>
        /// <returns> The query string with the url encoded search term. </returns>
        private static string GetMsdnFrontEndSearch(string searchTerm)
            => $"https://docs.microsoft.com/en-us/dotnet/api/?term={WebUtility.UrlEncode(searchTerm)}";

        /// <summary>
        ///     Gets the search results for a given search term.
        /// </summary>
        /// <param name="s"> The string to search for. </param>
        /// <returns> The resulting search terms, if any. </returns>
        private async Task<SearchResults> GetMsdnResultsAsync(string s)
        {
            // check for null parameter
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentNullException(nameof(s), "The supplied search term may not be null or whitespace.");
            }
            // trim whitespace, then check that it matches the regex for valid search terms
            s = s.Trim();
            if (!Regex.IsMatch(SearchTermRegex, s))
            {
                throw new ArgumentException(paramName: nameof(s), message:
                    "The search term contained invalid characters.");
            }
            using (var client = new HttpClient())
            {
                // GET the api query
                var result = await client.GetAsync(GetMsdnApiSearchUrl(s));
                if (result.IsSuccessStatusCode)
                {
                    // read the contents into a json reader
                    return JsonConvert.DeserializeObject<SearchResults>(await result.Content.ReadAsStringAsync());
                }
            }
            // non success status code
            return null;
        }
    }
}