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

            //Just for testing. Will be removed eventually
            //dayOfWeek = DayOfWeek.Saturday;
            //timeofDay = new TimeSpan(07, 0, 1);

            //FTF begins on Friday at 06:00 AM UTC. The logic will run if it's Friday or if it's Saturday, but hasn't hit the expiration time
            if ((dayOfWeek == DayOfWeek.Friday && timeofDay >= _actionConfig.StartUtcTime) || (dayOfWeek == DayOfWeek.Saturday && timeofDay < _actionConfig.EndUtcTime))
            {
                //Set spam filtering to All to make every post require approval
                SetSpamFiltering(SelfPostSpamFilterStrength.All);
                //Sets this week's FTF sticky post, if it hasn't already been made
                MakeStickyPost();
                //PM users to let them know about FTF
                PmUsersOnNewPosts();
            }

            //FTF ends Saturday at 06:00 AM UTC
            if (dayOfWeek == DayOfWeek.Saturday)
            {
                //This will check if the current time between the end time and the end time plus 1 hour. This extra one hour is to make sure the bot can still switch off FTF
                //if other tasks are taking a long time to process. We also don't want the bot pulling data for the unsticky when not needed and setting the spam filering to low
                //when it's not needed. We could accidentally override a mod's actions
                if (timeofDay >= _actionConfig.EndUtcTime || timeofDay >= _actionConfig.EndUtcTime.Add(new TimeSpan(1, 0, 0)))
                {
                    //Set spam filtering back to low after FTF is over
                    SetSpamFiltering(SelfPostSpamFilterStrength.Low);
                    //Unsticky the current free text friday post
                    UnstickyFreeTextFridayPost();
                }
            }
        }
        /// <summary>
        /// Check is self post spam filtering has already been set and sets it if not
        /// </summary>
        private void SetSpamFiltering(SelfPostSpamFilterStrength filter)
        {
            //if (_redditClient.GetSelfPostSpamFilterStrength() != filter)
                _redditClient.UpdateSubredditSpamFilter(filter);
        }
        /// <summary>
        /// Checks if this week's sticky post has been made. If not, it makes it.
        /// </summary>
        private void MakeStickyPost()
        {
            //The reddit client method only returns stickied posts. And the LINQ query grabs only the stickied posts whose titles match the title in the sticky post settings
            //I'm using .Any() instead of checking how many posts are stickied, because it ultimately doesn't matter. There can only ever be one stickied post.
            bool isPostAlreadyStickied = _redditClient.GetLastStickedPosts().Where(x => x.Title.Contains(_actionConfig.StickyPostSettings.Title)).Any();

            if (!isPostAlreadyStickied)
            {
                string title = $"{_actionConfig.StickyPostSettings.Title} - {DateTime.UtcNow.ToString("MM/dd/yy")}";
                _redditClient.SubmitStickyTextPost(title, _actionConfig.StickyPostSettings.Body, _actionConfig.StickyPostSettings.Flair);
            }
        }
        /// <summary>
        /// Sends a private messgae
        /// </summary>
        private void PmUsersOnNewPosts()
        {
            //We'll grab all of the posts from the new queue that don't have a flair already. This is so we can set the flair and PM the individual.
            var posts = _redditClient.GetRedditPostsFromNewQueue().Where(x => x.LinkFlairText != null && x.LinkFlairText.ToLower() != _actionConfig.FlairText.ToLower()).ToList();
            foreach (var post in posts)
            {
                var postDate = post.ApprovedAtUtc != null ? (DateTime)post.ApprovedAtUtc : post.CreatedUtc; //Use the approved at time when available because a post might be removed due to automod
                var todaysDate = DateTime.UtcNow.Date;

                //FTF will pull all new posts. There's a small chance that one of the new posts could be on Thursday if it was submitted late and there weren't a lot of posts made during that
                //thursday/friday window. So we want to compare the post creation date to "today's date." In general, "today's date" should always be Friday because that's when FTF runs.
                if (postDate.Date != todaysDate)
                    continue;

                if (post.UserName != _redditClient.GetBotUserName()) //Don't want the bot PMing itself
                {
                    //All of these bools and checks could be replaced by a single statements. But I wanted to make this as readable as possible and this
                    //felt like the most readable.

                    bool shouldPmUser = false;
                    bool isMod = _redditClient.GetModerators().Contains(post.UserName);
                    bool shouldExcludeMods = _actionConfig.PrivateMessageSettings.ExcludeMods;

                    //We only want to send the PM if the user is not a mod or if the user is a mod and mods are not excluded
                    if ((!isMod) || (isMod && !shouldExcludeMods))
                        shouldPmUser = true;

                    if (shouldPmUser && _actionConfig.PrivateMessageSettings.Enabled)
                    {
                        //Set the flair so we don't PM the users more than once.
                        post.SetFlair(_actionConfig.FlairText, _actionConfig.FlairCssClass);
                        //Don't PM if the setting is disabled. But we check it all the way at the end so that
                        _redditClient.SendPrivateMessage(_actionConfig.PrivateMessageSettings.Subject, _actionConfig.PrivateMessageSettings.Message, post.UserName);
                    }
                }
            }
        }
        /// <summary>
        /// Unstickies the bot's FTF posts
        /// </summary>
        private void UnstickyFreeTextFridayPost()
        {
            var posts = _redditClient.GetLastStickedPosts().Where(x => x.Title.Contains(_actionConfig.StickyPostSettings.Title));
            //There should only ever be one stickied post with the FTF text. But this is just in case something crazy happens
            foreach (var post in posts)
                post.UnstickyPost();
        }
    }
}
