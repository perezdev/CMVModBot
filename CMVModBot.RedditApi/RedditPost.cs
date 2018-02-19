using RedditSharp.Things;
using System;
using System.Collections.Generic;

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
            Url = post.Url.ToString();
            SelfText = _post.SelfText;
            Title = _post.Title;
            CreatedUtc = _post.CreatedUTC;
            UserName = _post.AuthorName;
            ApprovedAtUtc = _post.ApprovedAtUtc;
        }

        public string Id { get; set; }
        public bool IsSelfPost { get; set; }
        public string LinkFlairCssClass { get; set; }
        public string LinkFlairText { get; set; }
        public int CommentCount { get; set; }
        public Uri Permalink { get; set; }
        public string Url { get; set; }
        public string SelfText { get; set; }
        public string Title { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime? ApprovedAtUtc { get; set; }
        public string UserName { get; set; }
        public bool IsMod { get; set; }

        public void SetFlair(string flair)
        {
            _post.SetFlairAsync(flair, "").GetAwaiter().GetResult();
        }
        public void UnstickyPost()
        {
            _post.StickyModeAsync(false).GetAwaiter().GetResult();
        }
        public List<RedditComment> GetCommentsWithMore(int limit = 100, CommentThingSort sort = CommentThingSort.Qa)
        {
            var comments = new List<RedditComment>();

            var things = _post.GetCommentsWithMoresAsync(limit, GetCommentSortFromThingSort(sort)).GetAwaiter().GetResult();

            try
            {
                foreach (var thing in things)
                {
                    try
                    {
                        //I think we need to get rid of "more" eventually. I think I got confused on what more would return. This is a temp fix to allow the bot to continue running
                        if (thing.GetType() == typeof(Comment))
                            comments.Add(new RedditComment((Comment)thing));
                    }
                    catch (Exception ex)
                    {
                        //Ignore the error for now so the processing can continue   
                    }
                }
            }
            catch (Exception ex)
            {
                //Ignore the error for now so the processing can continue
            }
            
            return comments;
        }
        public List<RedditComment> GetComments(int limit = 100, CommentThingSort sort = CommentThingSort.Qa)
        {
            var comments = new List<RedditComment>();

            foreach (Comment comment in _post.GetCommentsAsync(limit, GetCommentSortFromThingSort(sort)).GetAwaiter().GetResult())
                comments.Add(new RedditComment(comment));

            return comments;
        }
        public void RemovePost()
        {
            _post.RemoveAsync().GetAwaiter().GetResult();
        }
        public RedditComment SubmitModComment(string message)
        {
            Comment comment = _post.CommentAsync(message).GetAwaiter().GetResult() as Comment;
            return new RedditComment(comment);
        }

        private CommentSort GetCommentSortFromThingSort(CommentThingSort sort)
        {
            switch (sort)
            {
                case CommentThingSort.Best:
                    return CommentSort.Best;
                case CommentThingSort.Top:
                    return CommentSort.Top;
                case CommentThingSort.New:
                    return CommentSort.New;
                case CommentThingSort.Controversial:
                    return CommentSort.Controversial;
                case CommentThingSort.Old:
                    return CommentSort.Old;
                case CommentThingSort.Qa:
                    return CommentSort.Qa;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
