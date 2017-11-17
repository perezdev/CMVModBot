using CMVModBot.RedditApi;
using Newtonsoft.Json;
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
        /// <summary>
        /// Gets the config object from the app.config file (for sensitive data) and the specified Wiki page
        /// </summary>
        /// <returns></returns>
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

            //Instantiating the reddit client logs the bot into reddit. Every reddit api action will be called from the reddit client
            var redditClient = new RedditClient(config.BotUsername, config.BotPassword, config.RedditApiClientId, config.RedditApiSecret, config.RedditApiRedirectUri, config.SubredditShortcut);
            //The wiki page stores the confguration as JSON
            var configJson = redditClient.GetWikiPageText(config.WikiPageName);
            //The sensitive config values like the bot password and API secret come from the app.config. Everything else is stored on
            //the wiki page so it can be updated when needed.
            var wikiConfig = DeserializeJson(configJson);
            //Update new config object with the values from the wiki config
            config.Enabled = wikiConfig.Enabled;
            //Gets the enabled sub action configs from the wiki
            config.SubredditActionConfigs = GetSubredditActionConfigsFromWiki(wikiConfig);

            return config;
        }
        /// <summary>
        /// Gets the enabled configurations for the subreddit actions from the wiki config
        /// </summary>
        /// <param name="wikiConfig"></param>
        /// <returns>List of enabled sub action configs</returns>
        private static List<SubActionConfigBase> GetSubredditActionConfigsFromWiki(Config wikiConfig)
        {
            var subActionConfigs = new List<SubActionConfigBase>();

            //We'll populate the subreddit action configs with defaults if none exist in the wiki
            if (wikiConfig.SubredditActionConfigs.Count == 0)
            {
                //Fresh Topic Friday
                var subActionConfig_FreeTopicFriday = new FreshTopicFridaySubActionConfig()
                {
                    Enabled = true,
                    Name = "Fresh Topic Friday",
                    FlairText = "Fresh Topic Friday",
                    StartDayOfWeek = DayOfWeek.Friday,
                    EndDayOfWeek = DayOfWeek.Saturday,
                    StartUtcTime = new TimeSpan(0, 6, 0, 0, 0),
                    EndUtcTime = new TimeSpan(0, 6, 0, 0, 0)
                };

                //SnooNotes

                //BanDiscussions

                //Rule E Violation

                subActionConfigs.Add(subActionConfig_FreeTopicFriday);
            }
            else //Otherwise, we'll get the existing configs
            {
                foreach (SubActionConfigBase wikiActionConfig in wikiConfig.SubredditActionConfigs)
                {
                    if (wikiActionConfig.Enabled)
                        subActionConfigs.Add(wikiActionConfig);
                }
            }

            return subActionConfigs;
        }

        /// <summary>
        /// Saves the config data to the wiki page. Not sure if I'm going to keep this method since the bot won't be updating it's own config info.
        /// </summary>
        /// <param name="config">Config object</param>
        public static void SaveConfig(Config config)
        {
            string configJson = SerializeJson(config); //Convert config object to JSON string

            var redditClient = new RedditClient(config.BotUsername, config.BotPassword, config.RedditApiClientId, config.RedditApiSecret, config.RedditApiSecret, config.SubredditShortcut);
            redditClient.SaveWikiPageText(config.WikiPageName, configJson);
        }
        /// <summary>
        /// Converts config object to a JSON string
        /// </summary>
        /// <param name="config">Config object</param>
        /// <returns></returns>
        private static string SerializeJson(Config config)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, Formatting = Formatting.Indented };
            var text = JsonConvert.SerializeObject(config, settings);

            return text;
        }
        /// <summary>
        /// Converts JSON string to config object
        /// </summary>
        /// <param name="json">JSON string</param>
        /// <returns></returns>
        private static Config DeserializeJson(string json)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            var configuration = JsonConvert.DeserializeObject<Config>(json, settings);

            return configuration;
        }
    }
}
