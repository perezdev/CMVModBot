using RedditSharp;

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
                    _reddit = new Reddit(webAgent, false);
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
                
                settings.UpdateSettings();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="subredditShortcut"></param>
        /// <returns></returns>
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
    }
    public enum SelfPostSpamFilterStrength
    {
        Low,
        High,
        All
    }
}
