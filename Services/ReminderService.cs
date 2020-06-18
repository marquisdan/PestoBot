using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using PestoBot.Common;
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
            //Filter into reminders that are due 
            //Send reminders if required
            //Log stuff
            throw new NotImplementedException();
        }

        private List<ReminderModel> GetListOfReminders(ReminderTypes type)
        {
            throw new NotImplementedException();
        }

        internal virtual bool ShouldSendTaskReminder(DateTime ReminderDueTime)
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
