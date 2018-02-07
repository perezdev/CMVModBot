using Newtonsoft.Json;

namespace CMVModBot.SnooNotes
{
    public class SnooNotesNoteType
    {
        [JsonProperty("NoteTypeID")]
        public int NoteTypeId { get; set; }
        [JsonProperty("SubName")]
        public string SubName { get; set; }
        [JsonProperty("DisplayName")]
        public string DisplayName { get; set; }
        [JsonProperty("ColorCode")]
        public string ColorCode { get; set; }
        [JsonProperty("DisplayOrder")]
        public int DisplayOrder { get; set; }
        [JsonProperty("Bold")]
        public bool Bold { get; set; }
        [JsonProperty("Italic")]
        public bool Italic { get; set; }
        [JsonProperty("IconString")]
        public string IconString { get; set; }
        [JsonProperty("Disabled")]
        public bool Disabled { get; set; }
    }
}
