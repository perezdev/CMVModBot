using Newtonsoft.Json;
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
        [JsonIgnore]
        public string BotUsername { get; set; }
        [JsonIgnore]
        public string BotPassword { get; set; }
        [JsonIgnore]
        public string RedditApiClientId { get; set; }
        [JsonIgnore]
        public string RedditApiSecret { get; set; }
        [JsonIgnore]
        public string RedditApiRedirectUri { get; set; }
        public string WikiPageName { get; set; }

        public List<SubActionConfigBase> SubredditActionConfigs { get; set; } = new List<SubActionConfigBase>();
    }
}
