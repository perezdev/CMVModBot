using Newtonsoft.Json;

namespace CMVModBot.SnooNotes
{
    public class SnooNotesSubreddit
    {
        [JsonProperty("SubredditID")]
        public int SubredditId { get; set; }
        [JsonProperty("SubName")]
        public string SubName { get; set; }
        [JsonProperty("Active")]
        public bool Active { get; set; }
        [JsonProperty("SentinelActive")]
        public bool SentinelActive { get; set; }
        [JsonProperty("Settings")]
        public SnooNotesSettings Settings { get; set; } = new SnooNotesSettings();
        [JsonProperty("IsAdmin")]
        public bool IsAdmin { get; set; }
    }
}
