using System;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WherwellCC.Contact
{
    public class GraphAPI
    {
        private static readonly HttpClient client = new HttpClient();

        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string ext_expires_in { get; set; }
        public string access_token { get; set; }

        public static async Task<GraphAPI> NewAccessToken(string clientId, string tenantName, string clientSecret) {
            Dictionary<string, string> ReqTokenBody = new Dictionary <string, string>
            {
                { "Grant_Type", "client_credentials" },
                { "Scope", "https://graph.microsoft.com/.default" },
                { "client_Id", clientId },
                { "client_Secret", clientSecret }
            };
            var content = new FormUrlEncodedContent(ReqTokenBody);
            var response = await client.PostAsync($"https://login.microsoftonline.com/{tenantName}/oauth2/v2.0/token", content);
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<GraphAPI>(responseString);
            return result;
        }

        public async Task<HttpResponseMessage> PostData(string path, string body)
        {
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + this.access_token);
            client.DefaultRequestHeaders.Add("Content-Type", "application/json");
            var response = await client.PostAsync(path, content);
            return response;
        }

    }
}