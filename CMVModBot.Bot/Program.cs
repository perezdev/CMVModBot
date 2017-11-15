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

        public static RedditClient _redditClient { get; set; }
        public static Config _config { get; set; }

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
                _config = ConfigManager.GetConfig(); //Config will be pulled fresh from the sub wiki on every iteration of work
                _redditClient = new RedditClient(_config.BotUsername, _config.BotPassword, _config.RedditApiClientId, _config.RedditApiSecret, _config.RedditApiRedirectUri);

                ConfigManager.SaveConfig(_config);
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }

            IsWorking = false;
        }
    }
}
