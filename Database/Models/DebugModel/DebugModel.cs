using SpeedathonBot.Database.Models.Common;

namespace SpeedathonBot.Database.Models.Debug
{
    public class DebugModel : AbstractPestoModel
    {
        //Simple model for testing DB 
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
