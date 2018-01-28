using CMVModBot.Configuration;
using CMVModBot.RedditApi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CMVModBot.Bot.SubredditActions
{
    /// <summary>
    /// https://github.com/perezdev/CMVModBot/issues/1
    /// </summary>
    public class RuleERemovalSubredditAction : SubredditActionBase
    {
        private RuleERemovalSubActionConfig _actionConfig { get; set; }
        private RedditClient _redditClient { get; set; }

        public RuleERemovalSubredditAction(RuleERemovalSubActionConfig actionConfig, RedditClient redditClient)
        {
            _actionConfig = actionConfig;
            _redditClient = redditClient;
        }
        
        public override void PerformSubredditAction()
        {
            var botName = _redditClient.GetBotUserName();
            var posts = _redditClient.GetRedditPostsFromNewQueue(200).Where(x => x.UserName != botName).ToList();
            var mods = _redditClient.GetModerators();
            
            var shouldExcludeMods = _actionConfig.ExcludeMods;

            foreach (var post in posts)
            {
                var isMod = mods.Contains(post.UserName);
                //1. If we shouldn't exclude mods from the rules, then we should perform action regardless if the user is a mod.
                //2. If we should exclude mods from the rules, then we should only perform actions if the user is not a mod
                if ((!shouldExcludeMods) || (shouldExcludeMods && !isMod ))
                {
                    var postTime = post.ApprovedAtUtc != null ? (DateTime)post.ApprovedAtUtc : post.CreatedUtc; //Use the approved at time when available because a post might be removed due to automod
                    var nowTime = DateTime.UtcNow; //Current time
                    var hoursLapsed = (nowTime - postTime).TotalHours; //Number of hours that have passed since the post was made
                    var limit = _actionConfig.TimeLimitToRemovePost; //The number of hours that are allowed to pass before the post is removed, if there are no replies by OP
                    var commentsToCheck = _actionConfig.NumberOfTopLevelCommentsToCheck; //We only apply the rule if at least 'n' amount of people have replied. OP shouldn't be penalized if no one has responded
                    var removalMessage = _actionConfig.RemovalMessage;

                    if (hoursLapsed >= limit.TotalHours)
                    {
                        //QA comments sorts comments by OP response first. This is a reliable way of checking if OP responded than potentially pulling thousands of comments
                        var comments = post.GetCommentsWithMore(10, CommentThingSort.Qa); 
                        if (comments.Count >= commentsToCheck)
                        {
                            var hasOpReplied = HasOpRepliedToAnswers(comments, post.UserName);
                            if (!hasOpReplied)
                            {
                                //If OP hasn't replied, we'll: remove the post, post a comment (distinguish/sticky), and add a SnooNote
                                post.RemovePost();
                                SubmitComment(post, removalMessage);
                            }
                        }
                    }
                }
            }
        }
        private bool HasOpRepliedToAnswers(List<RedditComment> comments, string opName)
        {
            bool hasOpReplied = false;

            foreach (var comment in comments) //Root comments
            {
                var subComments = comment.Comments;
                hasOpReplied = subComments.Any(x => x.AuthorName == opName); //Check all sub comments in the root comment to see if OP has replied
                if (hasOpReplied)
                    break; //We'll exit the loop if OP has replied so it doesn't hit another root comment where OP hasn't replied
            }

            return hasOpReplied;
        }
        private void SubmitComment(RedditPost post, string message)
        {
            //Notify OP post has been removed
            var comment = post.SubmitModComment(message);
            comment.Distinguish(true); //Distinguish and sticky mod message
        }
        private void SubmitSnooNote()
        {

        }
    }
}
