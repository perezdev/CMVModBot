using RedditSharp;
using RedditSharp.Things;
using System.Collections.Generic;
using System.Linq;

namespace CMVModBot.RedditApi
{
    /// <summary>
    /// The abstracted layer used for client like console apps to get reddit data from Reddit Sharp
    /// All asynchronous calls are converted to synchronous calls via .GetAwaiter().GetResult() so that the code doesn't complete before actions are done
    /// </summary>
    public class RedditClient
    {
        private string _userName;
        private string _password;
        private string _clientId;
        private string _secret;
        private string _redirectUri;
        private string _subredditShortcut;

        /// <summary>
        /// Private reddit object so we don't expose it to other code. We want the RedditClient to handle all interactions with the reddit object. This is
        /// so we can have a single place to manage authentication and what not
        /// </summary>
        Reddit _reddit = null;
        private Reddit reddit
        {
            get
            {
                //The reddit object is used in a bunch of different places. So it seemed best to just do the logic in the property
                //so it will instantiate a new object any time it's null. This makes it simpler than calling a method to get the logic.
                if (_reddit == null)
                {
                    var webAgent = new BotWebAgent(_userName, _password, _clientId, _secret, _redirectUri);
                    _reddit = new Reddit(webAgent, true);
                }

                return _reddit;
            }
            set
            {
                _reddit = value;
            }
        }

        public RedditClient(string userName, string password, string clientId, string secret, string redirectUri, string subredditShortcut)
        {
            _userName = userName;
            _password = password;
            _clientId = clientId;
            _secret = secret;
            _redirectUri = redirectUri;
            _subredditShortcut = subredditShortcut;
        }

        /// <summary>
        /// Gets the wiki page plain text by subreddit
        /// </summary>
        /// <param name="subredditShortcut">/r/subName</param>
        /// <param name="pageName">Name of wiki page to pull</param>
        /// <returns>Plain wiki page text</returns>
        public string GetWikiPageText(string pageName)
        {
            string text = string.Empty;

            var sub = reddit.GetSubredditAsync(_subredditShortcut).GetAwaiter().GetResult();
            if (sub != null)
            {
                var wiki = sub.GetWiki;
                var page = wiki.GetPageAsync(pageName).GetAwaiter().GetResult();

                text = page.MarkdownContent;
            }

            return text;
        }
        /// <summary>
        /// Saves text to a specific wiki page
        /// </summary>
        /// <param name="subredditShortcut">/r/subName</param>
        /// <param name="pageName">Page to save text to</param>
        /// <param name="content">Plain text of page contents</param>
        public void SaveWikiPageText(string pageName, string content)
        {
            var sub = reddit.GetSubredditAsync(_subredditShortcut).GetAwaiter().GetResult();
            if (sub != null)
            {
                var wiki = sub.GetWiki;
                wiki.EditPageAsync(pageName, content).GetAwaiter().GetResult();
            }
        }
        /// <summary>
        /// Updates subreddit spam filter to low, high, or all
        /// </summary>
        /// <param name="subredditShortcut">/r/subName</param>
        /// <param name="filter">Low, high, all</param>
        public void UpdateSubredditSpamFilter(SelfPostSpamFilterStrength filter)
        {
            var sub = reddit.GetSubredditAsync(_subredditShortcut).GetAwaiter().GetResult();
            if (sub != null)
            {
                var settings = sub.GetSettingsAsync().GetAwaiter().GetResult();
                switch (filter)
                {
                    case SelfPostSpamFilterStrength.Low:
                        settings.SpamFilter.SelfPostStrength = SpamFilterStrength.Low;
                        break;
                    case SelfPostSpamFilterStrength.High:
                        settings.SpamFilter.SelfPostStrength = SpamFilterStrength.High;
                        break;
                    case SelfPostSpamFilterStrength.All:
                        settings.SpamFilter.SelfPostStrength = SpamFilterStrength.All;
                        break;
                }

                settings.UpdateSettings().GetAwaiter().GetResult();
            }
        }
        /// <summary>
        /// Gets the currently set spam filter stregth (all, high, low) for text posts
        /// </summary>
        /// <returns>SelfPostSpamFilterStrength</returns>
        public SelfPostSpamFilterStrength? GetSelfPostSpamFilterStrength()
        {
            SelfPostSpamFilterStrength? filterStrength = null;

            var sub = reddit.GetSubredditAsync(_subredditShortcut).GetAwaiter().GetResult();
            if (sub != null)
            {
                var settings = sub.GetSettingsAsync().GetAwaiter().GetResult();
                switch (settings.SpamFilter.SelfPostStrength)
                {
                    case SpamFilterStrength.All:
                        filterStrength = SelfPostSpamFilterStrength.All;
                        break;
                    case SpamFilterStrength.High:
                        filterStrength = SelfPostSpamFilterStrength.High;
                        break;
                    case SpamFilterStrength.Low:
                        filterStrength = SelfPostSpamFilterStrength.Low;
                        break;
                }
            }

            return filterStrength;
        }
        /// <summary>
        /// Submits a text post and stickies it
        /// </summary>
        /// <param name="title">Title of post</param>
        /// <param name="text">Body of post</param>
        /// <param name="flair">Flair of post</param>
        public void SubmitStickyTextPost(string title, string text, string flair)
        {
            var sub = reddit.GetSubredditAsync(_subredditShortcut).GetAwaiter().GetResult();
            if (sub != null)
            {
                //Submit post
                Post post = sub.SubmitTextPostAsync(title, text).GetAwaiter().GetResult();
                //Set flair after post is made
                post.SubredditName = _subredditShortcut.Replace("/r/", ""); //Subreddit name object is null and it prevents the flair from being set. Setting it manually is a doable work around
                post.SetFlairAsync(flair, "").GetAwaiter().GetResult();
                //Sticky post after post is made
                post.StickyModeAsync(true).GetAwaiter().GetResult();
            }
        }
        /// <summary>
        /// Gets the latest posts made by the bot. Posts are order by descending created UTC date.
        /// </summary>
        /// <param name="limit">Max number of posts to return.</param>
        /// <returns>List of reddit posts</returns>
        public List<RedditPost> GetLastStickedPosts(int limit = 5)
        {
            var posts = new List<RedditPost>();

            reddit.User.GetPosts(Sort.New, limit, FromTime.All).Take(limit).ForEachAsync(post =>
            {
                if (post.IsStickied) //For this method, we only care about the posts that are stickied.
                {
                    posts.Add(new RedditPost(post));
                }
            }).GetAwaiter().GetResult();

            return posts.OrderByDescending(x => x.CreatedUtc).ToList();
        }
        /// <summary>
        /// Gets a specified number of posts from the new queue
        /// </summary>
        /// <param name="limit">Number of posts to retrieve</param>
        /// <returns>List of RedditPost</returns>
        public List<RedditPost> GetRedditPostsFromNewQueue(int limit = 100)
        {
            var posts = new List<RedditPost>();

            var sub = reddit.GetSubredditAsync(_subredditShortcut).GetAwaiter().GetResult();
            if (sub != null)
            {
                sub.GetPosts(Subreddit.Sort.New, limit).Take(limit).ForEachAsync(post =>
                {
                    posts.Add(new RedditPost(post));
                }).GetAwaiter().GetResult();
            }
            
            return posts;
        }
        /// <summary>
        /// Sends a private message
        /// </summary>
        /// <param name="subject">Subject</param>
        /// <param name="body">Text</param>
        /// <param name="to">Reddit user name</param>
        public void SendPrivateMessage(string subject, string body, string to)
        {
            _reddit.ComposePrivateMessageAsync(subject, body, to).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Gets the bot's user name from the internal reddit object
        /// </summary>
        /// <returns></returns>
        public string GetBotUserName()
        {
            return _reddit.User.Name;
        }
        /// <summary>
        /// Gets a list of moderators
        /// </summary>
        /// <returns></returns>
        public List<string> GetModerators()
        {
            var mods = new List<string>();

            var sub = reddit.GetSubredditAsync(_subredditShortcut).GetAwaiter().GetResult();
            if (sub != null)
            {
                foreach(var mod in sub.GetModeratorsAsync().GetAwaiter().GetResult())
                    mods.Add(mod.Name);
            }

            return mods;
        }
    }
    public enum SelfPostSpamFilterStrength
    {
        Low,
        High,
        All
    }
}
