namespace PestoBot.Database.Models.Common
{
    public abstract class AbstractPestoModel : IPestoModel
    {
        public ulong Id { get; set; }
    }
}
