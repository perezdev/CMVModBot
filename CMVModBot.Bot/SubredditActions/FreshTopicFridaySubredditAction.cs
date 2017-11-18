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

            //dayOfWeek = DayOfWeek.Friday; //Testing

            //FTF begins on Friday at 06:00 AM UTC. The logic will run if it's Friday or if it's Saturday, but hasn't hit the expiration time
            if ((dayOfWeek == DayOfWeek.Friday && timeofDay >= _actionConfig.StartUtcTime) || (dayOfWeek == DayOfWeek.Saturday && timeofDay < _actionConfig.EndUtcTime))
            {
                //Set spam filtering to All to make every post require approval
                SetSpamFiltering();

                //Sets this week's FTF sticky post, if it hasn't already been made
                MakeStickyPost();
            }

            //FTF ends Saturday at 06:00 AM UTC
            if (dayOfWeek == DayOfWeek.Saturday) 
            {
                //Only process if current time is at least 6 AM UTC
            }
        }
        /// <summary>
        /// Check is self post spam filtering has already been set and sets it if not
        /// </summary>
        private void SetSpamFiltering()
        {
            if (_redditClient.GetSelfPostSpamFilterStrength() != SelfPostSpamFilterStrength.All)
                _redditClient.UpdateSubredditSpamFilter(SelfPostSpamFilterStrength.All);
        }
        /// <summary>
        /// Checks if this week's sticky post has been made. If not, it makes it.
        /// </summary>
        private void MakeStickyPost()
        {
            _redditClient.SubmitStickyTextPost(_actionConfig.StickyPostSettings.Title, _actionConfig.StickyPostSettings.Body, _actionConfig.StickyPostSettings.Flair);
        }
    }
}
