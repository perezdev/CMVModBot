using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CMVModBot.SnooNotes
{
    public class SnooNotesAuthProvider
    {
        private readonly string _username;
        private readonly string _password;
        private const string TokenUrl = "https://snoonotes.com/auth/connect/token";

        public SnooNotesAuthProvider(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public SnooNotesLoginTokenResult GetLoginTokenResult()
        {
            string content = $"grant_type=password&username={HttpUtility.UrlEncode(_username)}&password={_password}&client_id=bots";

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(TokenUrl);
            HttpResponseMessage response = client.PostAsync("Token", new StringContent(content, Encoding.UTF8, "application/x-www-form-urlencoded")).Result;

            string resultJSON = response.Content.ReadAsStringAsync().Result;
            SnooNotesLoginTokenResult result = JsonConvert.DeserializeObject<SnooNotesLoginTokenResult>(resultJSON);

            return result;
        }
    }
}
