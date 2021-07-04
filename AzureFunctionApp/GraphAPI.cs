using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WherwellCC.Contact
{
    public class GraphAPI
    {
        public GraphAPI(string json)
        {

        }
        public string token_type { get; set; }
        public string expires_in { get; set; }
        public string ext_expires_in { get; set; }
        public string access_token { get; set; }
    }
}