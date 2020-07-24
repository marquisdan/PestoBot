using System;
using PestoBot.Database.Models.SpeedrunEvent;
using PestoBot.Database.Repositories.SpeedrunEvent;

namespace PestoBot.Api.Common
{
    class MarathonTaskAssignmentApi : AbstractPestoApi<MarathonTaskAssignmentModel>
    {
        private MarathonTaskAssignmentModel _model;
        private readonly MarathonTaskAssignmentRepository _repo;

        public MarathonTaskAssignmentApi()
        {
            _model = new MarathonTaskAssignmentModel
            {
                Created = DateTime.Now
            };

            _repo = new MarathonTaskAssignmentRepository();
        }

        public MarathonTaskAssignmentApi(MarathonTaskAssignmentModel model)
        {
            _model = model;
            _repo = new MarathonTaskAssignmentRepository();
        }
    }
}