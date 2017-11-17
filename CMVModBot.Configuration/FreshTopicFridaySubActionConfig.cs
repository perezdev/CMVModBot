﻿using System;

namespace CMVModBot.Configuration
{
    public class FreshTopicFridaySubActionConfig : SubActionConfigBase
    {
        public string FlairText { get; set; }
        public DayOfWeek StartDayOfWeek { get; set; }
        public DayOfWeek EndDayOfWeek { get; set; }
        public TimeSpan StartUtcTime { get; set; }
        public TimeSpan EndUtcTime { get; set; }
    }
}
