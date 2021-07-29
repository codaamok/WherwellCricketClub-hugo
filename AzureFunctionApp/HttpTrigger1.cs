using System;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.IO;
using System.Web;
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

            var data = HttpUtility.ParseQueryString(requestBody);
            // JObject data = JsonConvert.DeserializeObject<JObject>(requestBody);

            if (Array.Exists(data.AllKeys, element => element == "warmup"))
            {
                log.LogInformation("Warm up received");
                return new OkResult();
            }

            log.LogInformation("Printing all POST'ed data:");

            foreach (var item in data)
            {
                log.LogInformation($"- {item}: {data[item.ToString()]}");
            }

            string message = $@"<p><b>Name:</b> {data["senderName"].ToString()}</p>
<p><b>Email:</b> {data["senderEmailAddress"].ToString()}</p>
<p><b>Message:</b> {data["message"].ToString()}</p>
<p><i>Replying to this email will be replying to the sender using their address above.</i></p>";

            GraphAPIMailClient mail = new GraphAPIMailClient(
                subject: "Website Contact",
                content: message,
                toRecipients: new Dictionary<string, string> 
                { 
                    { "admin@wherwellcc.co.uk", "Wherwell Cricket Club" }
                },
                user: "admin@wherwellcc.co.uk"
            );

            try {
                await mail.CreateDraft(log);
            }
            catch (Exception ex) {
                log.LogError($"Failed to create message: {ex.Message.ToString()}");
                // return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                return new RedirectResult("https://new.wherwellcc.co.uk/contactfailed");
            }

            try {
                await mail.UpdateMessage(
                    log: log,
                    replyToRecipients: new Dictionary<string, string> 
                    { 
                        { data["senderEmailAddress"].ToString(), data["senderName"].ToString() }
                    }
                );
            }
            catch (Exception ex) {
                log.LogError($"Failed to update message: {ex.Message.ToString()}");
                // return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                return new RedirectResult("https://new.wherwellcc.co.uk/contactfailed");
            }

            try {
                await mail.SendMessage(log);
            }
            catch (Exception ex) {
                log.LogError($"Failed to send message: {ex.Message.ToString()}");
                //return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                return new RedirectResult("https://new.wherwellcc.co.uk/contactfailed");
            }

            //return new OkResult();
            return new RedirectResult("https://new.wherwellcc.co.uk/contactsuccess");
        }
    }
}
