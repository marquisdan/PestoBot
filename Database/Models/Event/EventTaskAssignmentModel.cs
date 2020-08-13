using System;
using PestoBot.Database.Models.Common;

namespace PestoBot.Database.Models.Event
{
    public class EventTaskAssignmentModel : AbstractPestoModel
    {
        public int AssignmentType { get; set; }
        public DateTime ProjectDueDate { get; set; }
        public DateTime TaskStartTime { get; set; }
        public string ReminderText { get; set; }
        public DateTime LastReminderSent { get; set; }
        public ulong EventTaskId { get; set; }
        public ulong UserId { get; set; }
        public ulong MarathonTaskId { get; set; }
        public ulong EventId { get; set; }
    }
}
