using CMVModBot.Bot.SubredditActions;
using CMVModBot.Configuration;
using CMVModBot.RedditApi;
using System;
using System.Threading;

namespace CMVModBot.Bot
{
    class Program
    {
        private static Timer _serviceTimer { get; set; }
        private static bool IsWorking { get; set; } = false;

        public static RedditClient redditClient { get; set; }
        public static Config config { get; set; }

        public static void Main(string[] args)
        {
            DoWork();
        }

        private static void DoWork()
        {
            try
            {
                config = ConfigManager.GetConfig(); //Config will be pulled fresh from the sub wiki on every iteration of work
                redditClient = new RedditClient(config.BotUsername, config.BotPassword, config.RedditApiClientId, config.RedditApiSecret, config.RedditApiRedirectUri, config.SubredditShortcut);

                if (config.Enabled)
                {
                    foreach (var actionConfig in config.SubredditActionConfigs)
                    {
                        if (actionConfig.GetType() == typeof(FreshTopicFridaySubActionConfig))
                            new FreshTopicFridaySubredditAction(actionConfig as FreshTopicFridaySubActionConfig, redditClient).PerformSubredditAction();
                        else if (actionConfig.GetType() == typeof(RuleERemovalSubActionConfig))
                            new RuleERemovalSubredditAction(actionConfig as RuleERemovalSubActionConfig, redditClient).PerformSubredditAction();
                    }
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
        }
    }
}
