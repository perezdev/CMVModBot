using RedditSharp.Things;
using System;

namespace CMVModBot.RedditApi
{
    public class RedditPost
    {
        private Post _post { get; set; }

        public RedditPost() { }
        public RedditPost(Post post)
        {
            _post = post;

            Id = _post.Id;
            CommentCount = _post.CommentCount;
            IsSelfPost = _post.IsSelfPost;
            LinkFlairCssClass = _post.LinkFlairCssClass;
            LinkFlairText = _post.LinkFlairText;
            Permalink = _post.Permalink;
            SelfText = _post.SelfText;
            Title = _post.Title;
            CreatedUtc = _post.CreatedUTC;
            UserName = _post.AuthorName;
        }

        public string Id { get; set; }
        public bool IsSelfPost { get; set; }
        public string LinkFlairCssClass { get; set; }
        public string LinkFlairText { get; set; }
        public int CommentCount { get; set; }
        public Uri Permalink { get; set; }
        public string SelfText { get; set; }
        public string Title { get; set; }
        public DateTime CreatedUtc { get; set; }
        public string UserName { get; set; }

        public void SetFlair(string flair)
        {
            _post.SetFlairAsync(flair, "").GetAwaiter().GetResult();
        }
        public void UnstickyPost()
        {
            _post.StickyModeAsync(false).GetAwaiter().GetResult();
        }
    }
}
