using PestoBot.Database.Models.Common;

namespace PestoBot.Database.Models.DebugModel
{
    public class DebugModel : AbstractPestoModel
    {
        //Simple model for testing DB 
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
