using System;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
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
    public class GraphAPIClient
    {
        private static readonly HttpClient client = new HttpClient();

        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string ext_expires_in { get; set; }
        public string access_token { get; set; }

        public async Task<GraphAPIClient> NewAccessToken(
            string clientId, 
            string tenantName, 
            string clientSecret, 
            ILogger log
        )
        {
            Dictionary<string, string> ReqTokenBody = new Dictionary <string, string>
            {
                { "Grant_Type", "client_credentials" },
                { "Scope", "https://graph.microsoft.com/.default" },
                { "client_Id", clientId },
                { "client_Secret", clientSecret }
            };
            var content = new FormUrlEncodedContent(ReqTokenBody);
            log.LogInformation("Getting new access token");
            var response = await client.PostAsync($"https://login.microsoftonline.com/{tenantName}/oauth2/v2.0/token", content);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GraphAPIClient>(responseString);
        }

        public async Task<HttpResponseMessage> SendData(
            string method, 
            string path, 
            string body
        )
        {
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            HttpResponseMessage response = new HttpResponseMessage();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.access_token);

            switch (method)
            {
                case "POST":
                    response = await client.PostAsync(path, content);
                    break;
                case "PATCH":
                    response = await client.PatchAsync(path, content);
                    break;
            }
            
            return response;
        }

    }
}