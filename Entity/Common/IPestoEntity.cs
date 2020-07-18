namespace PestoBot.Entity.Common
{
    interface IPestoEntity<T>
    {
        void Load(ulong id);
        void Save();
    }
}
