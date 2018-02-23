using CMVModBot.Configuration;
using CMVModBot.RedditApi;
using CMVModBot.SnooNotes;
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
        private SnooNotesClient _snooNotesClient { get; set; }

        public RuleERemovalSubredditAction(RuleERemovalSubActionConfig actionConfig, RedditClient redditClient, SnooNotesClient snooNotesClient)
        {
            _actionConfig = actionConfig;
            _redditClient = redditClient;
            _snooNotesClient = snooNotesClient;
        }

        public override void PerformSubredditAction()
        {
            var botName = _redditClient.GetBotUserName();
            var posts = _redditClient.GetRedditPostsFromNewQueue(200).Where(x => x.UserName != botName).ToList();
            var mods = _redditClient.GetModerators();

            var shouldExcludeMods = _actionConfig.ExcludeMods;

            foreach (var post in posts)
            {
                var postTime = post.ApprovedAtUtc != null ? (DateTime)post.ApprovedAtUtc : post.CreatedUtc; //Use the approved at time when available because a post might be removed due to automod

                var isMod = mods.Contains(post.UserName);
                //1. If we shouldn't exclude mods from the rules, then we should perform action regardless if the user is a mod.
                //2. If we should exclude mods from the rules, then we should only perform actions if the user is not a mod
                if ((!shouldExcludeMods) || (shouldExcludeMods && !isMod))
                {
                    //-----------------Previous Rule E Violation Check-----------------

                    var snooNotesRuleENoteTypeId = GetSnooNotesRuleETypeId();
                    var snooNotes = _snooNotesClient.GetSnooNotesByUserName(post.UserName);
                    var hasRuleBeenBroken = HasOpBrokenRuleBefore(post.UserName, snooNotesRuleENoteTypeId, snooNotes);
                    if (hasRuleBeenBroken)
                    {
                        //We need to determine if OP has replied to an old post where they violated Rule E. We grab the first post here because it doesn't matter if they have or haven't
                        //replied to multiple posts. Only that they haven't replied to at least one. We're excluding notes for this post because we don't want to post a comment
                        //for a post where we've already submitted a note
                        //We order by descending so we can get the last post that violated the rule. If we didn't sort, it would give us the first post and newer posts could be let through by accident
                        var ruleESnooNote = snooNotes.Where(x => x.NoteTypeId == snooNotesRuleENoteTypeId && x.Url != post.Url).OrderByDescending(x => x.Timestamp).FirstOrDefault();
                        //This would only be null in the case where they only had one rule e violation and we're on it
                        //The month check is because reddit archives posts after 6 months. So if the note is older than 6 months, then we're not going to hold it against them
                        //The postTime and snoo note timestamp check is to make sure that the post was made after the snoo note was made. We don't want to remove posts that didn't violate the rules before the snoo note was created
                        if (ruleESnooNote != null && (DateTime.UtcNow - ruleESnooNote.Timestamp).Days < 183 && postTime > ruleESnooNote.Timestamp)
                        {
                            bool hasOpRepliedToPreviousPost = HasOpRepliedToPreviousRuleEPost(ruleESnooNote.Url);
                            if (!hasOpRepliedToPreviousPost)
                            {
                                post.RemovePost();
                                var alreadyRemovedRemovalMessage = _actionConfig.SnooNotesSettings.PreviousRuleEViolationMessage.Replace("&lt;link&gt;", ruleESnooNote.Url);
                                alreadyRemovedRemovalMessage = alreadyRemovedRemovalMessage.Replace("<link>", ruleESnooNote.Url);
                                alreadyRemovedRemovalMessage = alreadyRemovedRemovalMessage.Replace("&amp;lt;username&amp;gt;", post.UserName);
                                alreadyRemovedRemovalMessage = alreadyRemovedRemovalMessage.Replace("&lt;username&gt;", post.UserName);
                                alreadyRemovedRemovalMessage = alreadyRemovedRemovalMessage.Replace("<username>", post.UserName);
                                alreadyRemovedRemovalMessage = alreadyRemovedRemovalMessage.Replace("&gt;", ">");

                                SubmitComment(post, alreadyRemovedRemovalMessage);

                                continue; //Since the post gets removed when the user has already been broken the rule, we'll stop here and continue with the next post
                            }
                        }
                    }

                    //-----------------Current Rule E Violation Check-----------------

                    var nowTime = DateTime.UtcNow; //Current time
                    var hoursLapsed = (nowTime - postTime).TotalHours; //Number of hours that have passed since the post was made
                    var limit = _actionConfig.TimeLimitToRemovePost; //The number of hours that are allowed to pass before the post is removed, if there are no replies by OP
                    var commentsToCheck = _actionConfig.NumberOfTopLevelCommentsToCheck; //We only apply the rule if at least 'n' amount of people have replied. OP shouldn't be penalized if no one has responded
                    var removalMessage = _actionConfig.RemovalMessage.Replace("&amp;lt;username&amp;gt;", post.UserName);
                    removalMessage = removalMessage.Replace("&lt;username&gt;", post.UserName);
                    removalMessage = removalMessage.Replace("<username>", post.UserName);
                    removalMessage = removalMessage.Replace("&gt;", ">");

                    if (hoursLapsed >= limit.TotalHours)
                    {
                        //QA comments sorts comments by OP response first. This is a reliable way of checking if OP responded than potentially pulling thousands of comments
                        //Typically, a post won't already have bot comments. But sometimes a bot comment will be made and then removed if the post is later approved. So we need
                        //to exclude comments made by the bot, just in case
                        var comments = post.GetComments(10, CommentThingSort.Qa).Where(x => x.AuthorName != botName && x.IsRemoved != null && x.IsRemoved == false).ToList();
                        if (comments.Count >= commentsToCheck)
                        {
                            var hasOpReplied = HasOpRepliedToAnswers(comments, post.UserName);
                            if (!hasOpReplied)
                            {
                                //If OP hasn't replied, we'll: remove the post, post a comment (distinguish/sticky), and add a SnooNote
                                post.RemovePost();
                                SubmitComment(post, removalMessage);
                                SubmitNewSnooNote(post.UserName, post.Url, snooNotesRuleENoteTypeId);
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
        private void SubmitNewSnooNote(string name, string url, int noteTypeId)
        {
            _snooNotesClient.SubmitSnooNote(name, "Rule E Violation", url, noteTypeId);
        }
        private int GetSnooNotesRuleETypeId()
        {
            try
            {
                var subreddit = _snooNotesClient.GetSnooNotesSubreddit();
                var noteType = subreddit.Settings.NoteTypes.SingleOrDefault(x => x.DisplayName == _actionConfig.SnooNotesSettings.RuleERuleName);
                return noteType.NoteTypeId;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        /// <summary>
        /// Determines if the user has previously broken the rule by snoo notes
        /// </summary>
        /// <param name="url">Current post</param>
        /// <param name="names"></param>
        /// <returns></returns>
        private bool HasOpBrokenRuleBefore(string name, int noteTypeId, List<SnooNote> userSnooNotes)
        {
            bool broken = false;

            if (userSnooNotes != null && userSnooNotes.Any(x => x.NoteTypeId == noteTypeId)) //broken is true when any of the notes match the notetypeid from the subreddit settings
                broken = true;

            return broken;
        }
        /// <summary>
        /// Determines if OP has replied to the old rule e violated post, based on the snoo notes settings
        /// </summary>
        /// <param name="url">Full URL to post</param>
        /// <returns>bool</returns>
        private bool HasOpRepliedToPreviousRuleEPost(string url)
        {
            var minimumComments = _actionConfig.SnooNotesSettings.PreviousRuleEViolationMinimumComments; //Number of comments that OP has to make
            var minimumCommentLength = _actionConfig.SnooNotesSettings.PreviousRuleEViolationMinimumCommentLength; //Length that each comment has to be, for violating Rule E

            var post = _redditClient.GetRedditPostByUrl(url);
            var comments = post.GetCommentsWithMore(10, CommentThingSort.Qa);

            var opcomments = new List<RedditComment>();
            foreach (var comment in comments) //Top level comments
            {
                foreach (var opcomment in comment.Comments) //OP has to reply to top level comments. So we don't need to recurse through all of the commments
                {
                    //Only add the comment to the list if it was made by OP and it's at least as long as the config states.
                    if (opcomment.AuthorName == post.UserName && opcomment.Body.Length >= minimumCommentLength)
                        opcomments.Add(opcomment);
                }
            }

            return opcomments.Count >= minimumComments;
        }
    }
}
