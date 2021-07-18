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

            GraphAPIClient GraphAPIClient = new GraphAPIClient();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            JObject data = JsonConvert.DeserializeObject<JObject>(requestBody);

            if (data["warmup"].ToString() == "true")
            {
                log.LogInformation("Warm up received");
                return new OkResult();
            }

            try {
                GraphAPIClient = await GraphAPIClient.NewAccessToken(
                    System.Environment.GetEnvironmentVariable("AAD_APP_ID_WHERWELLCC_ADMIN"),
                    "cookadamcouk.onmicrosoft.com",
                    System.Environment.GetEnvironmentVariable("AAD_APP_SECRET_WHERWELLCC_ADMIN"),
                    log
                );
            }
            catch (Exception ex) {
                log.LogError($"Failed to request access token: {ex.Message.ToString()}");
            }

            log.LogInformation("Printing all POST'ed data:");

            foreach (var item in data)
            {
                log.LogInformation($"- {item.Key}: {item.Value}");
            }

            var body = new {
                Subject = "Contact Us",
                Body = new {
                    ContentType = "Text",
                    Content = String.Join("\n", data["message"].ToString())
                },
                ToRecipients = new[] {
                    new {
                        EmailAddress = new {
                            Address = "me@cookadam.co.uk"
                        },
                    },
                    new {
                        EmailAddress = new {
                            Address = "adamcook807@gmail.com"
                        },
                    },
                },
                Sender = new {
                    EmailAddress = new {
                        Address = "web@wherwellcc.co.uk",
                        Name = "Website Inquiry"
                    }
                },
                From = new {
                    EmailAddress = new {
                        Address = "web@wherwellcc.co.uk",
                        Name = "Website Inquiry"
                    }
                },
                ReplyTo = new[] {
                    new {
                        EmailAddress = new {
                            Address = "mayavthomas@outlook.com"
                        }
                    }
                },
                SaveToSentItems = true,
                isDraft = false,
            };

            var tosend = JsonConvert.SerializeObject(body);
            
            HttpResponseMessage result = new HttpResponseMessage();

            try {
                result = await GraphAPIClient.SendData("POST", "https://graph.microsoft.com/v1.0/users/web@wherwellcc.co.uk/20210717.1844@cookadam.co.uk/createReply", tosend);
                result.EnsureSuccessStatusCode();
                log.LogInformation("Successfully sent message");
                return new RedirectResult("https://new.wherwellcc.co.uk/contactsuccess.html");
            }
            catch (Exception ex) {
                log.LogError($"Failed to send message: {ex.Message.ToString()}");
                return new RedirectResult("https://new.wherwellcc.co.uk/contactfailed.html");
            }
        }
    }
}
