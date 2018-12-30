using Discord;
using Newtonsoft.Json;
using Patek.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Patek.Services
{
    public class ApiSearchService : IDisposable
    {
        /// <summary>
        ///     Up to the first N results to display in the embed.
        /// </summary>
        private const int NumResultsToDisplay = 3;

        /// <summary>
        ///     Regular expression that determines what search queries are valid.
        ///     This is stated in error messages when invalid terms are searched.
        /// </summary>
        private static Regex SearchTermRegex = new Regex(@"^[A-Za-z][A-Za-z0-9\\.<>,]+$");

        private HttpClient _http;

        public ApiSearchService()
        {
            _http = new HttpClient();
        }

        /// <summary>
        ///     Gets the query string of the docs.microsoft.com api browser with the search parameter populated.
        /// </summary>
        /// <param name="searchTerm"> The term to search for. Assumes not null or whitespace. </param>
        /// <returns> The query string with the url encoded search term. </returns>
        public static string GetMsdnApiSearchUrl(string searchTerm)
            => $"https://docs.microsoft.com/api/apibrowser/dotnet/search?search={WebUtility.UrlEncode(searchTerm)}";

        /// <summary>
        ///     Gets the query string of the front-end url for the given search parameter.
        ///     This is what users will see in their web browser.
        /// </summary>
        /// <param name="searchTerm"> The term to search for. Assumes not null or whitespace. </param>
        /// <returns> The query string with the url encoded search term. </returns>
        public static string GetMsdnFrontEndSearch(string searchTerm)
            => $"https://docs.microsoft.com/en-us/dotnet/api/?term={WebUtility.UrlEncode(searchTerm)}";

        /// <summary>
        ///     Gets the search results for a given search term.
        /// </summary>
        /// <param name="searchTerm"> The string to search for. </param>
        /// <returns> The resulting search terms, if any. </returns>
        public async Task<MsdnSearchResults> GetMsdnResultsAsync(string searchTerm)
        {
            // check for null parameter
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                throw new ArgumentNullException(nameof(searchTerm), "The supplied search term may not be null or whitespace.");
            }
            // trim whitespace, then check that it matches the regex for valid search terms
            searchTerm = searchTerm.Trim();
            if (!SearchTermRegex.IsMatch(searchTerm))
            {
                throw new ArgumentException(paramName: nameof(searchTerm), message:
                    "The search term contained invalid characters.");
            }
            
            // GET the api query
            var result = await _http.GetAsync(GetMsdnApiSearchUrl(searchTerm));
            if (result.IsSuccessStatusCode)
            {
                // read the contents into a json reader
                return JsonConvert.DeserializeObject<MsdnSearchResults>(await result.Content.ReadAsStringAsync());
            }
            // non success status code
            return null;
        }

        /// <summary>
        ///     Builds an Embed for the given SearchResults.
        /// </summary>
        /// <param name="results"> The search results to display in the embed. Assumed not null. </param>
        /// <param name="searchTerm"> The search term that produced these results. Assumed not null. </param>
        /// <returns> A new Embed instance containing the search results information. </returns>
        public static Embed BuildResultsEmbed(MsdnSearchResults results, string searchTerm)
        {
            var eb = new EmbedBuilder();
            eb.WithCurrentTimestamp();
            eb.WithTitle($".NET API Search: \"{searchTerm}\"");
            // .NET purple
            eb.WithColor(new Color(104, 33, 122));

            // use the embed description to show all results
            // making the assumption that I don't need to worry about the embed description field
            // limit
            var sb = new StringBuilder();
            foreach (var x in results.Results.Take(NumResultsToDisplay))
            {
                // the Description field should be HTML decoded, as often &lt; and &rt; are used in descriptions
                sb.AppendLine($"{x.ItemKind} [{x.DisplayName}]({x.Url})\n{WebUtility.HtmlDecode(x.Description)}\n");
            }
            // include a link to the website itself that includes all results
            sb.AppendLine($"[View all results in the .NET API Browser]({ApiSearchService.GetMsdnFrontEndSearch(searchTerm)})");
            eb.WithDescription(sb.ToString());

            return eb.Build();
        }

        public void Dispose()
        {
            _http.Dispose();
        }
    }
}
