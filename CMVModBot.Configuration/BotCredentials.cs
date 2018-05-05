using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMVModBot.Configuration
{
    public class BotCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ClientId { get; set; }
        public string ApiSecret { get; set; }
        public string RedirectUri { get; set; }
        public string SubredditShortcut { get; set; }
    }
}
