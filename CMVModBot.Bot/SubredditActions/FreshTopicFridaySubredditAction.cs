using CMVModBot.Configuration;
using CMVModBot.RedditApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMVModBot.Bot.SubredditActions
{
    /// <summary>
    /// https://github.com/perezdev/CMVModBot/issues/4
    /// </summary>
    public class FreshTopicFridaySubredditAction : SubredditActionBase
    {
        private FreshTopicFridaySubActionConfig _actionConfig { get; set; }
        private Config _config { get; set; }
        private RedditClient _redditClient { get; set; }

        public FreshTopicFridaySubredditAction(FreshTopicFridaySubActionConfig actionConfig, RedditClient redditClient)
        {
            _actionConfig = actionConfig;
            _redditClient = redditClient;
        }
        /// <summary>
        /// Peforms all the necessary checks and options to enable and disable fresh topic friday
        /// </summary>
        public override void PerformSubredditAction()
        {
            var dayOfWeek = DateTime.UtcNow.DayOfWeek;
            var timeofDay = DateTime.UtcNow.TimeOfDay;

            //FTF begins on Friday at 06:00 AM UTC
            if (dayOfWeek == DayOfWeek.Friday)
            {
                if (timeofDay >= _actionConfig.StartUtcTime) //Only process if current time is at least 6 AM UTC
                {
                    if (_redditClient.GetSelfPostSpamFilterStrength() != SelfPostSpamFilterStrength.All)
                        _redditClient.UpdateSubredditSpamFilter(SelfPostSpamFilterStrength.All);
                }
            }

            //FTF ends Saturday at 06:00 AM UTC
            if (dayOfWeek == DayOfWeek.Saturday) 
            {
                //Only process if current time is at least 6 AM UTC
            }
        }
    }
}
