using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using PestoBot.Common;
using PestoBot.Database.Models.Common;
using PestoBot.Database.Models.SpeedrunEvent;
using PestoBot.Database.Repositories.SpeedrunEvent;
using Serilog;

namespace PestoBot.Services
{
    public enum ReminderTimes
    {
        Project = 17, //Fire daily reminders at 5pm
        GoobyTime = 20 //Fire Gooby reminder at 8pm
    }

    public class ReminderService
    {
        private const int Minute = 60000;
        private const int Hour = Minute * 60;
        private const int Day = Hour * 24;
        internal const int TaskReminderTime = 30; //Remind tasks this many minutes before due date
        private const string LogDateFormat = "MMMM dd, yyyy HH:mm:ss tt zz";

        //Times for specific reminder types

        private Timer reminderTimer;

        public void Start()
        {
            reminderTimer = new Timer(WakeReminderService, null, 0, Minute * 5);
        }

        internal void WakeReminderService(object state)
        {
            Log.Information($"{DateTime.Now.ToString(LogDateFormat)} : Waking Reminder Service");
            Log.Information("Processing tasks");
            Log.Information($"{DateTime.Now.ToString(LogDateFormat)} : Reminder Service Sleeping");
        }

        protected internal void FireRemindersByType()
        {
            ProcessReminders(ReminderTypes.Task);
            if (IsTimeToProcessReminders(ReminderTimes.Project))
            {
                ProcessReminders(ReminderTypes.Project);
            }

            if (IsTimeToProcessReminders(ReminderTimes.GoobyTime))
            {
                ProcessReminders(ReminderTypes.Debug);
            }
        }

        protected internal virtual bool IsTimeToProcessReminders(ReminderTimes reminderTime)
        {
            var now = GetCurrentTime();
            return now.Hour == (int) reminderTime && now.Minute < 30;
        }

        protected internal virtual void ProcessReminders(ReminderTypes type)
        {
            //Get list of reminders
            var reminders = GetListOfReminders(type);
            //Filter into reminders that are due 
            var dueReminders = reminders.Where(x => ShouldSendReminder(GetDueDate(x)));
            //Send reminders if required
            foreach (var reminder in dueReminders)
            {
                SendReminder(reminder);
            }
            //Log stuff
        }

        private List<ReminderModel> GetListOfReminders(ReminderTypes type)
        {
            var repo = new ReminderRepository();
            return repo.GetListOfReminders(type).Result;
            throw new NotImplementedException();
        }

        internal virtual bool ShouldSendReminder(DateTime ReminderDueTime)
        {
            throw new NotImplementedException();
        }

        protected internal virtual DateTime GetDueDate(ReminderModel model)
        {
            switch ((ReminderTypes) model.Type) {
                case ReminderTypes.Task:
                    return GetShortTermDueDate(model);
                case ReminderTypes.Project:
                    return GetLongTermDueDate(model);
                case ReminderTypes.Run:
                    return GetShortTermDueDate(model);
                case ReminderTypes.Debug:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns a short term due date for Tasks or Runs
        /// These are reminders that occur a certain time before a scheduled run or task
        /// e.g. Tell a runner to get ready 30m before a run
        /// </summary>
        /// <returns></returns>
        protected internal DateTime GetShortTermDueDate(ReminderModel model)
        {
            var assignment = GetAssignmentForReminder(model) as MarathonTaskAssignmentModel;
            throw new NotImplementedException();
        }

        protected internal DateTime GetLongTermDueDate(ReminderModel model)
        {
            var assignment = GetAssignmentForReminder(model) as MarathonProjectAssignmentModel;
            var project = GetProjectForAssignment(assignment);

            throw new NotImplementedException();
        }

        protected internal virtual IPestoModel GetAssignmentForReminder(ReminderModel model)
        {
            throw new NotImplementedException();
        }

        protected internal virtual MarathonProjectModel GetProjectForAssignment(MarathonProjectAssignmentModel model)
        {
            throw new NotImplementedException();
        }

        protected internal virtual DateTime GetCurrentTime()
        {
            return DateTime.Now;
        }

        protected internal void SendReminder(ReminderModel reminder)
        {
            throw new NotImplementedException();
        }
    }
}
