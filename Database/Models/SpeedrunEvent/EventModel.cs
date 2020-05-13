using System;
using PestoBot.Database.Models.Common;

namespace PestoBot.Database.Models.SpeedrunEvent
{
    public class EventModel : AbstractPestoModel
    {
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ScheduleCloseDate { get; set; }
        public string ScheduleUrl { get; set; }
        public string ApplicationUrl { get; set; }
        public string Charity { get; set; }
        public string CharityUrl { get; set; }
        public string DonationUrl { get; set; }
        public ulong CreatorId { get; set; }
        public string CreatorUsername { get; set; }
        public ulong GuildId { get; set; }
    }
}