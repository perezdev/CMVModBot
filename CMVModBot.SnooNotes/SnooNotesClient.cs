using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace CMVModBot.SnooNotes
{
    public class SnooNotesClient
    {
        internal SnooNotesLoginTokenResult _result;

        private const string GetNotesUrl = "https://snoonotes.com/api/Note/GetNotes";
        private const string PostNoteUrl = "https://snoonotes.com/api/note";
        private const string GetNoteTypesUrl = "https://snoonotes.com/restapi/Subreddit";
        private const string GetSubredditUrl = "https://snoonotes.com/restapi/Subreddit";

        internal string _subredditName { get; set; }

        public SnooNotesClient(string username, string password, string subredditName)
        {
            //We need to set the sub name to get the correct data. The replaces are messy, but are the common sub prefixes and it'll cause problems if they are there.
            _subredditName = subredditName.Replace("/r/", "").Replace("r/", "");

            var auth = new SnooNotesAuthProvider(username, password);
            _result = auth.GetLoginTokenResult();
        }

        public SnooNotesSubreddit GetSnooNotesSubreddit()
        {
            var client = GetHttpClient();
            var result = client.GetAsync(new Uri($"{GetSubredditUrl}/{_subredditName}")).GetAwaiter().GetResult();
            //The JSON is returned as an array for some reason, even though it's a single object. The easiest way to fix this was just remove the opening and ending bracket
            var json = result.Content.ReadAsStringAsync().GetAwaiter().GetResult().TrimStart('[').TrimEnd(']');

            var subreddit = JsonConvert.DeserializeObject<SnooNotesSubreddit>(json); //I got JSON property tagging on the first try. Suck it, JSON objects

            client.Dispose();

            return subreddit;
        }

        /// <summary>
        /// Queries SnooNotes API by array of user names
        /// </summary>
        /// <returns></returns>
        public List<SnooNote> GetSnooNotesByUserName(string name)
        {
            List<SnooNote> snooNotes = null;

            var client = GetHttpClient();

            //We make a post with the get notes URL and pass the users as an array of strings. It has to be an array, or it will fail
            var result = client.PostAsync(new Uri(GetNotesUrl), new string[] { name }.AsJson()).GetAwaiter().GetResult();
            if (result.StatusCode == HttpStatusCode.OK)
            {
                var rootJson = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                //https://stackoverflow.com/a/23648388
                //The gist of this issue is that the object has a dynamic root. I must complain to meep about this. He loves hearing my complaints
                //So I "fixed" it by following that SO link.
                var rootJsonObject = JObject.Parse(rootJson);
                if (rootJsonObject != null && rootJsonObject.HasValues)
                {
                    var rootProperties = rootJsonObject.Properties().First(); //The "first" object is the root which is the user's name. The actual notes are under it
                                                                              //Don't judge me. It was quick and it works.
                    var json = rootProperties.Value.ToString().Trim(); //I don't think the trim is needed. But I'm too lazy to check

                    snooNotes = JsonConvert.DeserializeObject<List<SnooNote>>(json);
                    //Since the name is the root property, it doesn't get populated into the snoonote object. I don't think is is actually needed. But ¯\_(ツ)_/¯
                    foreach (var note in snooNotes)
                        note.Name = rootProperties.Name;
                }
            }

            client.Dispose();

            return snooNotes;
        }

        public void SubmitSnooNote(string name, string message, string url, int noteTypeId)
        {
            var data = new
            {
                NoteTypeID = noteTypeId,
                SubName = _subredditName,
                Message = message,
                AppliesToUsername = name,
                Url = url,
            };

            var client = GetHttpClient();
            var result = client.PostAsync(new Uri(PostNoteUrl), data.AsJson()).GetAwaiter().GetResult();
            client.Dispose();
        }
        /// <summary>
        /// HTTP client that's used for all the SnooNotes calls. Adds authorization.
        /// </summary>
        /// <returns>HttpClient with authorization</returns>
        private HttpClient GetHttpClient()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _result.ToString());
            return client;
        }
    }
    public static class Extensions
    {
        public static StringContent AsJson(this object o)
         => new StringContent(JsonConvert.SerializeObject(o), Encoding.UTF8, "application/json");
    }
}
