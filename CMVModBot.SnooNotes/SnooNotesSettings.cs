using Newtonsoft.Json;
using System.Collections.Generic;

namespace CMVModBot.SnooNotes
{
    public class SnooNotesSettings
    {
        [JsonProperty("AccessMask")]
        public int AccessMask { get; set; }
        [JsonProperty("NoteTypes")]
        public List<SnooNotesNoteType> NoteTypes { get; set; } = new List<SnooNotesNoteType>();
        [JsonProperty("PermBanID")]
        public int PermBanId { get; set; }
        [JsonProperty("TempBanID")]
        public int TempBanId { get; set; }
    }
}
