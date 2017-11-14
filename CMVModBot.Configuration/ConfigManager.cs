using CMVModBot.RedditApi;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMVModBot.Configuration
{
    public static class ConfigManager
    {
        public static Config GetConfig()
        {
            var config = new Config()
            {
                SubredditShortcut = ConfigurationManager.AppSettings["SubredditShortcut"],
                BotUsername = ConfigurationManager.AppSettings["BotUsername"],
                BotPassword = ConfigurationManager.AppSettings["BotPassword"],
                RedditApiClientId = ConfigurationManager.AppSettings["RedditApiClientId"],
                RedditApiSecret = ConfigurationManager.AppSettings["RedditApiSecret"],
                RedditApiRedirectUri = ConfigurationManager.AppSettings["RedditApiRedirectUri"],
                WikiPageName = ConfigurationManager.AppSettings["WikiPageName"]
            };

            var redditClient = new RedditClient(config.BotUsername, config.BotPassword, config.RedditApiClientId, config.RedditApiSecret, config.RedditApiSecret);
            var wiki = redditClient.GetWikiPageText(config.SubredditShortcut, config.WikiPageName);

            return config;
        }
        public static void SaveConfig(Config config)
        {
            var redditClient = new RedditClient(config.BotUsername, config.BotPassword, config.RedditApiClientId, config.RedditApiSecret, config.RedditApiSecret);
            redditClient.SaveWikiPageText(config.SubredditShortcut, config.WikiPageName, "Save from CMVModBot Test");
        }
    }
}
