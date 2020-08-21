using System;

public class ReminderRecord
{
    public DateTime Scheduled { get; set; }
    public string Game { get; set; }
    public string Category { get; set; }
    public string VolunteerType { get; set; }
    public string DiscordUserName { get; set; }
    public int Sent { get; set; }
}