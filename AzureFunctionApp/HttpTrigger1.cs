using System;
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

    public static class HttpTrigger1
    {
        private static readonly HttpClient client = new HttpClient();

        [FunctionName("HttpTrigger1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            JObject data = JsonConvert.DeserializeObject<JObject>(requestBody);

            if (data["warmup"].ToString() == "true")
            {
                log.LogInformation("Warm up received");
                return new OkResult();
            }

            string ClientId = System.Environment.GetEnvironmentVariable("AAD_APP_ID_WHERWELLCC_ADMIN");
            string TenantName = "cookadamcouk.onmicrosoft.com";
            string ClientSecret = System.Environment.GetEnvironmentVariable("AAD_APP_SECRET_THECOOKS_WEDDING");
            string Resource = "https://graph.microsoft.com/";

            Dictionary<string, string> ReqTokenBody = new Dictionary <string, string>
            {
                { "Grant_Type", "client_credentials" },
                { "Scope", "https://graph.microsoft.com/.default" },
                { "client_Id", ClientId },
                { "client_Secret", ClientSecret }
            };

            log.LogInformation("Requesting access token");

            var content = new FormUrlEncodedContent(ReqTokenBody);

            var response = await client.PostAsync($"https://login.microsoftonline.com/{TenantName}/oauth2/v2.0/token", content);

            var responseString = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<GraphAPI>(responseString);
            
            return new OkResult();
        }
    }
}
