using Newtonsoft.Json;
using System.Collections.Generic;

namespace CMVModBot.Configuration
{
    /// <summary>
    /// This config is saved to the wiki. Sensitive properties are ignored so we're not saving password, API keys, etc.
    /// </summary>
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
        [JsonIgnore]
        public string SnooNotesApiKey { get; set; }
        [JsonIgnore]
        public string SnooNotesUsername { get; set; }

        public List<SubActionConfigBase> SubredditActionConfigs { get; set; } = new List<SubActionConfigBase>();
    }
}
