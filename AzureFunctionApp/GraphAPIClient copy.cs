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
    public class GraphAPIMailClient
    {
        public string id { get; set; }
        public string subject { get; set; }
        public string content { get; set; }
        public string[] toRecipients { get; set; }
        public string[] replyTo { get; set; }

        public void CreateMessage (
            string subject,
            string content,
            string[] toRecipients
        )
        {  

        }
        public void UpdateMessage (
            string subject = this.subject,
            string content = this.content,
            string[] toRecipients = this.toRecipients
        )
        {

        }
        public void SendMessage (

        )
        {

        }
    }
}