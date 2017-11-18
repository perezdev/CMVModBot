using CMVModBot.Bot.SubredditActions;
using CMVModBot.Configuration;
using CMVModBot.RedditApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            //_serviceTimer = new Timer(ServiceTimerCallback, null, 0, 1000);
            //Console.ReadLine();
            DoWork();
        }

        private static void ServiceTimerCallback(Object sender)
        {
            //The bot will continously run every second. But won't do any work until the previous work has completed.
            if (!IsWorking)
            {
                DoWork();
            }
        }

        private static void DoWork()
        {
            IsWorking = true;

            try
            {
                config = ConfigManager.GetConfig(); //Config will be pulled fresh from the sub wiki on every iteration of work
                redditClient = new RedditClient(config.BotUsername, config.BotPassword, config.RedditApiClientId, config.RedditApiSecret, config.RedditApiRedirectUri, config.SubredditShortcut);

                if (config.Enabled)
                {
                    //Configs won't be in the list if they aren't enabled from the wiki. So that's why I'm doing an Any LINQ check
                    if (config.SubredditActionConfigs.Any(x => x.GetType() == typeof(FreshTopicFridaySubActionConfig)))
                    {
                        //Single linq check because there will only ever be one of each config. And if we're at this point, then the config exists.
                        var actionConfig = config.SubredditActionConfigs.Single(x => x.GetType() == typeof(FreshTopicFridaySubActionConfig)) as FreshTopicFridaySubActionConfig;
                        var action_FTF = new FreshTopicFridaySubredditAction(actionConfig, redditClient);
                        action_FTF.PerformSubredditAction();
                    }
                }

                //ConfigManager.SaveConfig(config);
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }

            IsWorking = false;
        }
    }
}
