using Newtonsoft.Json;
using System;

namespace CMVModBot.SnooNotes
{
    public class SnooNote
    {
        [JsonIgnore]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "NoteID")]
        public int NoteId { get; set; }
        [JsonProperty(PropertyName = "NoteTypeID")]
        public int NoteTypeId { get; set; }
        [JsonProperty(PropertyName = "SubName")]
        public string SubName { get; set; }
        [JsonProperty(PropertyName = "Submitter")]
        public string Submitter { get; set; }
        [JsonProperty(PropertyName = "Message")]
        public string Message { get; set; }
        [JsonProperty(PropertyName = "Url")]
        public string Url { get; set; }
        [JsonProperty(PropertyName = "Timestamp")]
        public DateTime Timestamp { get; set; }
        [JsonProperty(PropertyName = "ParentSubreddit")]
        public string ParentSubreddit { get; set; }
    }
}
