using CMVModBot.Bot.SubredditActions;
using CMVModBot.Configuration;
using CMVModBot.RedditApi;
using CMVModBot.SnooNotes;
using System;

namespace CMVModBot.Bot
{
    class Program
    {
        public static RedditClient _reddit { get; set; }
        public static Config _config { get; set; }
        public static SnooNotesClient _snoonotes { get; set; }

        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                bool shouldTestReddit = false;
                bool shouldTestSnooNotes = false;

                foreach (var arg in args)
                {
                    if (arg.ToLower() == "/TestReddit".ToLower())
                        shouldTestSnooNotes = true;
                    else if (arg.ToLower() == "/TestSnooNotes".ToLower())
                        shouldTestSnooNotes = true;
                }

                //if (shouldTestReddit)
                //    TestReddit();
                //if (shouldTestSnooNotes)
                //    TestSnooNotes();
            }
            else
            {
                DoWork();
            }
        }

        private static void DoWork()
        {
            try
            {
                var credentials = ConfigManager.GetBotCredentials();
                _reddit = new RedditClient(credentials.Username, credentials.Password, credentials.ClientId, credentials.ApiSecret, credentials.RedirectUri, credentials.SubredditShortcut);
                _config = ConfigManager.GetConfig(_reddit); //Config will be pulled fresh from the sub wiki on every iteration of work
                
                _snoonotes = new SnooNotesClient(_config.SnooNotesUsername, _config.SnooNotesApiKey, _config.SubredditShortcut);

                if (_config.Enabled)
                {
                    foreach (var actionConfig in _config.SubredditActionConfigs)
                    {
                        Console.WriteLine($"{actionConfig.Name} - {actionConfig.Enabled}");

                        if (actionConfig.GetType() == typeof(FreshTopicFridaySubActionConfig) && actionConfig.Enabled)
                            new FreshTopicFridaySubredditAction(actionConfig as FreshTopicFridaySubActionConfig, _reddit).PerformSubredditAction();
                        else if (actionConfig.GetType() == typeof(RuleERemovalSubActionConfig) && actionConfig.Enabled)
                            new RuleERemovalSubredditAction(actionConfig as RuleERemovalSubActionConfig, _reddit, _snoonotes).PerformSubredditAction();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        //private static void TestReddit()
        //{

        //}
        //private static void TestSnooNotes()
        //{
        //    try
        //    {
        //        Console.WriteLine("Starting SnooNotes connection test...");

        //        Console.WriteLine("Getting configuration...");
        //        _config = ConfigManager.GetConfig();
        //        Console.WriteLine("Creating SnooNotes client...");
        //        var client = new SnooNotesClient(_config.SnooNotesUsername, _config.SnooNotesApiKey, _config.SubredditShortcut);
        //        Console.WriteLine("Performing SnooNotes GET...");
        //        var notes = client.GetSnooNotesSubreddit();
        //        if (notes == null)
        //            Console.WriteLine("Error: SnooNotes GET returned NULL object...");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //}
    }
}
