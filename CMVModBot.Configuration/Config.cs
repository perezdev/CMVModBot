using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMVModBot.Configuration
{
    public class Config
    {
        public bool Enabled { get; set; } = true;
        public string SubredditShortcut { get; set; }
        public string BotUsername { get; set; }
        public string BotPassword { get; set; }
        public string RedditApiClientId { get; set; }
        public string RedditApiSecret { get; set; }
        public string RedditApiRedirectUri { get; set; }
        public string WikiPageName { get; set; }
    }
}
