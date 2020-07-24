using System;

namespace PestoBot.Database.Models.Common
{
    public abstract class AbstractPestoModel : IPestoModel
    {
        public ulong Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
}
