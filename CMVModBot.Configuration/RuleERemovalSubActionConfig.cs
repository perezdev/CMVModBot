using System;

namespace CMVModBot.Configuration
{
    public class RuleERemovalSubActionConfig : SubActionConfigBase
    {
        public TimeSpan TimeLimitToRemovePost { get; set; }
        public bool AddSnooNotes { get; set; }
        public int NumberOfTopLevelCommentsToCheck { get; set; }
        public string RemovalMessage { get; set; }
        public bool ExcludeMods { get; set; }
    }
}
