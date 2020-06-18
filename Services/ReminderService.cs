using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using PestoBot.Database.Models.SpeedrunEvent;
using Serilog;

namespace PestoBot.Services
{
    public class ReminderService
    {
        private const int Minute = 60000;
        private const int Hour = Minute * 60;
        private const int Day = Hour * 24;
        internal const int TaskReminderTime = 30; //Remind tasks this many minutes before due date
        private const string LogDateFormat = "MMMM dd, yyyy HH:mm:ss tt zz";

        private Timer _taskReminder;
        private Timer _projectReminder;

        static ReminderService()
        {
            //timer = new Timer(wake, null, 0,  1000 * 60 * 60 * 24); //24 hour interval
           // timer = new Timer(wake, null, 0,  1000 * 60); //1 minute interval
            
        }

        public void Start()
        {
            _taskReminder = new Timer(WakeTaskReminder, null, 0, Minute * 5);
            _projectReminder = new Timer(WakeProjectReminder, null, 0, Day);
        }

        internal void WakeTaskReminder(object state)
        {
            Log.Information($"{DateTime.Now.ToString(LogDateFormat)} : Project Reminder Service Woke up");
            Log.Information("Processing tasks");
            Log.Information($"{DateTime.Now.ToString(LogDateFormat)} : Project Reminder Service Sleeping");
        }

        private void ProcessTasks()
        {
            //Get list of task reminders
            //Send reminders if required
            //Log stuff
            throw new NotImplementedException();
        }

        private List<ReminderModel> GetListOfTaskReminders()
        {
            throw new NotImplementedException();
        }

        internal virtual bool ShouldSendTaskReminder(DateTime ReminderDueTime)
        {
            throw new NotImplementedException();
        }

        internal virtual DateTime GetCurrentTime()
        {
            return DateTime.Now;
        }

        static void WakeProjectReminder(object state)
        {
            Log.Information($"{DateTime.Now.ToString(LogDateFormat)} : Project Reminder Service Woke up");
            Log.Information("Processing projects");
            Log.Information($"{DateTime.Now.ToString(LogDateFormat)} : Project Reminder Service Sleeping");
        }
    }
}
