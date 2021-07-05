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

    public static class HttpTrigger1
    {
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

            var token = await GraphAPI.NewAccessToken(
                System.Environment.GetEnvironmentVariable("AAD_APP_ID_WHERWELLCC_ADMIN"),
                "cookadamcouk.onmicrosoft.com",
                System.Environment.GetEnvironmentVariable("AAD_APP_SECRET_THECOOKS_WEDDING")
            );

            log.LogInformation("Printing all POST'ed data:");

            foreach (var item in data)
            {
                log.LogInformation($"- {item.Key}: {item.Value}");
            }

            var body = new {
                message = new {
                    Subject = "Hello world",
                    Importance = "High",
                    Body = new {
                        ContentType = "Text",
                        Content = "This is some sample text"
                    },
                    ToRecipients = new {
                        EmailAddress = new {
                            Address = "me@cookadam.co.uk"
                        }
                    },
                },
            };

            var tosend = JsonConvert.SerializeObject(body);
            
            var x = token.PostData("https://graph.microsoft.com/v1.0/users/me@cookadam.co.uk/sendMail", tosend);

            return new OkResult();
        }
    }
}
