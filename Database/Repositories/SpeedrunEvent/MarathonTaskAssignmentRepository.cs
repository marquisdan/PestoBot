using System;
using System.Collections.Generic;
using System.Text;
using PestoBot.Database.Models.SpeedrunEvent;
using PestoBot.Database.Repositories.Common;

namespace PestoBot.Database.Repositories.SpeedrunEvent
{ 
    internal class MarathonTaskAssignmentRepository : AbstractPestoRepository<MarathonTaskAssignmentModel>
    {
        internal MarathonTaskAssignmentRepository()
        {
            TableName = "MarathonTaskAssignment";
        }
}
}
