namespace PestoBot.Entity.Common
{
    public interface IPestoEntity<T>
    {
        void Load(ulong id);
        void Save();
    }
}
