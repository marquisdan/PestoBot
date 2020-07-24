using System;

namespace PestoBot.Database.Models.Common
{
    public interface IPestoModel
    {
        ulong Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
}
