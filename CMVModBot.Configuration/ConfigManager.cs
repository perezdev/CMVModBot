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
        //This is so we can use the sub shortcut while building the config. Otherwise, we'd have to wait until the config was completed. Which won't work,
        //because some things require the sub shortcut like the PM info. This feels a bit hacky. But I'll address it later, if needed.
        private static string _subShortcut { get; set; }

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
                WikiPageName = ConfigurationManager.AppSettings["WikiPageName"],
                SnooNotesUsername = ConfigurationManager.AppSettings["SnooNotesUsername"],
                SnooNotesApiKey = ConfigurationManager.AppSettings["SnooNotesApiKey"],
            };

            _subShortcut = config.SubredditShortcut;

            //Instantiating the reddit client logs the bot into reddit. Every reddit api action will be called from the reddit client
            var redditClient = new RedditClient(config.BotUsername, config.BotPassword, config.RedditApiClientId, config.RedditApiSecret, config.RedditApiRedirectUri, config.SubredditShortcut);
            //The wiki page stores the confguration as JSON. If the JSON is empty, we'll build the config from default values
            var configJson = redditClient.GetWikiPageText(config.WikiPageName);
            //The sensitive config values like the bot password and API secret come from the app.config. Everything else is stored on
            //the wiki page so it can be updated when needed.
            var wikiConfig = DeserializeJson(configJson);
            //Update new config object with the values from the wiki config
            config.Enabled = wikiConfig == null ? true : wikiConfig.Enabled;
            //Gets the enabled sub action configs from the wiki
            config.SubredditActionConfigs = GetSubredditActionConfigsFromWiki(wikiConfig);

            //If the wiki config is null, it means the JSON stored in the wiki has been deleted. This tells us to create a new default config.
            if (wikiConfig == null)
                SaveConfig(config);

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
            if (wikiConfig == null || wikiConfig.SubredditActionConfigs.Count == 0)
            {
                //Fresh Topic Friday
                subActionConfigs.Add(GetDefaultFreshTopicFridaySubActionConfig());

                //Rule E Violations
                subActionConfigs.Add(GetDefaultRuleERemovalSubActionConfig());

                //Ban Discussions

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
        /// Saves the config data to the wiki page.
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

        #region Action Configs

        private static RuleERemovalSubActionConfig GetDefaultRuleERemovalSubActionConfig()
        {
            var actionConfig = new RuleERemovalSubActionConfig()
            {
                AddSnooNotes = true,
                Enabled = true,
                Name = "Rule E Removal",
                NumberOfTopLevelCommentsToCheck = 3,
                TimeLimitToRemovePost = new TimeSpan(3, 0, 0),
                RemovalMessage = GetRuleERemovalMessage(),
                SnooNotesSettings = new SnooNotesSettings()
                {
                    RuleERuleName = "Rule E",
                    PreviousRuleEViolationMessage = GetPreviousRuleEViolationRemovalMessage(),
                    PreviousRuleEViolationMinimumComments = 2,
                    PreviousRuleEViolationMinimumCommentLength = 100,
                },
            };

            return actionConfig;
        }
        private static FreshTopicFridaySubActionConfig GetDefaultFreshTopicFridaySubActionConfig()
        {
            var actionConfig = new FreshTopicFridaySubActionConfig()
            {
                Enabled = true,
                Name = "Fresh Topic Friday",
                FlairText = "FRESH TOPIC FRIDAY",
                FlairCssClass = "FTF",
                StartDayOfWeek = DayOfWeek.Friday,
                EndDayOfWeek = DayOfWeek.Saturday,
                StartUtcTime = new TimeSpan(0, 6, 0, 0, 0),
                EndUtcTime = new TimeSpan(0, 6, 0, 0, 0),
                StickyPostSettings = new StickyPostSettings()
                {
                    Enabled = true,
                    Title = $"It's Fresh Topic Friday!", //When we make the actual post, this will be appending with the current date
                    Body = GetFtfStickyPostBody(),
                    Flair = ""
                },
                PrivateMessageSettings = new PrivateMessageSettings()
                {
                    Enabled = true,
                    ExcludeMods = true,
                    Subject = "Fresh Topic Friday",
                    Message = GetFtfPmMessage()
                }
            };

            return actionConfig;
        }
        private static string GetFtfStickyPostBody()
        {
            var stickyPostBody = new StringBuilder();
            stickyPostBody.AppendLine("Every Friday, posts are withheld for review by the moderators and approved if they aren't highly similar to another made in the past month.");
            stickyPostBody.AppendLine();
            stickyPostBody.AppendLine("This is to reduce topic fatigue for our regular contributors, without which the subreddit would be worse off.");
            stickyPostBody.AppendLine();
            stickyPostBody.AppendLine("[See here](https://www.reddit.com/r/changemyview/wiki/freshtopicfriday) for a full explanation of Fresh Topic Friday.");
            stickyPostBody.AppendLine();
            stickyPostBody.AppendLine("*Feel free to [message the moderators](https://www.reddit.com/message/compose?to=%2Fr%2Fchangemyview) if you have any questions or concerns.*");

            return stickyPostBody.ToString();
        }
        private static string GetFtfPmMessage()
        {
            var message = new StringBuilder();
            message.AppendLine("Hello!");
            message.AppendLine();
            message.AppendLine("It's currently Fresh Topic Friday at r/changemyview, which means your post is awaiting review by the moderators. You won't receive any responses in the meantime, but a decision should be made shortly. If the topic is deemed too popular for FTF, you'll have to post another day of the week.");
            message.AppendLine();
            message.AppendLine($"For more information on FTF, please see the [stickied post]({_subShortcut}/about/sticky?num=1).");
            message.AppendLine();
            message.AppendLine("*I am a bot, and this action was performed automatically. If you have any questions or concerns, please [contact the moderators directly.](https://www.reddit.com/message/compose?to=%2Fr%2Fchangemyview)*");

            return message.ToString();
        }
        private static string GetRuleERemovalMessage()
        {
            var message = new StringBuilder();
            message.AppendLine("Sorry, u/&lt;username&gt; - your submission has been removed for breaking rule E:");
            message.AppendLine();
            message.AppendLine("> Only post if you are willing to have a conversation with those who reply to you, and are available to start doing so within 3 hours of posting. ");
            message.Append("If you haven't replied within this time, your post will be removed. [See the wiki for more information](http://www.reddit.com/r/changemyview/wiki/rules#wiki_rule_e).");
            message.AppendLine();
            message.AppendLine();
            message.AppendLine("If you would like to appeal, please respond substantially to some of the arguments people have made, and then message the [moderators by clicking this link](https://www.reddit.com/message/compose?to=%2Fr%2Fchangemyview).");

            return message.ToString();
        }
        private static string GetPreviousRuleEViolationRemovalMessage()
        {
            var message = new StringBuilder();
            message.AppendLine("Sorry, u/&lt;username&gt; - your post to CMV has been removed due to a [previous post](<link>) of yours breaking Rule E.");
            message.AppendLine("This is a fundamental rule, as CMV is all about having a conversation. [See the wiki for more information](https://www.reddit.com/r/changemyview/wiki/rules#wiki_rule_e).");
            message.AppendLine();
            message.AppendLine("If you wish to continue with your new post, you first must **respond to some comments in your previous post, and then resubmit.** CMVModBot will make sure a reasonable effort has been made before approving the resubmission.");
            message.AppendLine();
            message.Append("Please [message the moderators](https://www.reddit.com/message/compose?to=%2Fr%2Fchangemyview) if you have any questions or concerns.");
            message.AppendLine();

            return message.ToString();
        }
        #endregion
    }
}
