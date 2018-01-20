using RedditSharp.Things;
using System.Collections.Generic;

namespace CMVModBot.RedditApi
{
    public class RedditComment
    {
        private Comment _comment { get; set; }

        public string Id { get; set; }
        public string AuthorName { get; set; }
        public string Body { get; set; }
        public List<RedditComment> Comments { get; set; } = new List<RedditComment>();

        public RedditComment() { }
        public RedditComment(Comment comment)
        {
            _comment = comment;

            Id = _comment.Id;
            AuthorName = _comment.AuthorName;
            Body = _comment.Body;
            foreach (Comment c in _comment.Comments)
                Comments.Add(new RedditComment(c));
        }
        public void Distinguish(bool sticky = false)
        {
            _comment.DistinguishAsync(ModeratableThing.DistinguishType.Moderator, sticky).GetAwaiter().GetResult();
        }
    }
}
