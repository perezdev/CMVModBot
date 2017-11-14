using RedditSharp;
using RedditSharp.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        Reddit _reddit = null;
        private Reddit reddit
        {
            get
            {
                //The reddit object is used in a bunch of different places. So it seemed best to just do the logic in the property
                //so it will instantiate a new object any time it's null. This makes it simpler than calling a method to get the logic. And I don't want to expose the Reddit
                //object because then the console will need a direct reference to RedditSharp.
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

        public RedditClient(string userName, string password, string clientId, string secret, string redirectUri)
        {
            _userName = userName;
            _password = password;
            _clientId = clientId;
            _secret = secret;
            _redirectUri = redirectUri;
        }

        public string GetWikiPageText(string subredditShortct, string pageName)
        {
            string text = string.Empty;

            var sub = reddit.GetSubredditAsync(subredditShortct).GetAwaiter().GetResult();
            if (sub != null)
            {
                var wiki = sub.GetWiki;
                var page = wiki.GetPageAsync(pageName).GetAwaiter().GetResult();

                text = page.MarkdownContent;
            }

            return text;
        }
        public void SaveWikiPageText(string subredditShortct, string pageName, string content)
        {
            var sub = reddit.GetSubredditAsync(subredditShortct).GetAwaiter().GetResult();
            if (sub != null)
            {
                var wiki = sub.GetWiki;
                //var page = wiki.GetPageAsync(pageName).GetAwaiter().GetResult();
                wiki.EditPageAsync(pageName, content).GetAwaiter().GetResult();
            }
        }
    }
}
