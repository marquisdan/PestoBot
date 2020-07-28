using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using PestoBot.Common;
using PestoBot.Database.Models.SpeedrunEvent;
using PestoBot.Services;
using Serilog;

namespace PestoBot.Tests.ServiceTests
{
    class ReminderServiceTest
    {
        class ShouldSendOneTimeReminderTest
        {
            private ReminderService _sut;
            private DateTime _currentTime;
            private DateTime _dueDate;
            private EventTaskAssignmentModel eventTaskAssignment;

            [SetUp]
            public void SetUp()
            {
                _currentTime = DateTime.Parse("October 29, 2019 16:30:00");
                var provider = new ServiceCollection().BuildServiceProvider();
                var mockSut = new Mock<ReminderService>(provider) { CallBase = true };
                mockSut.Setup(x => x.InitServices(It.IsAny<IServiceProvider>()));
                mockSut.Setup(x => x.CreateReminderServiceLoggerConfiguration()).Returns(new LoggerConfiguration().CreateLogger);
                mockSut.Setup(x => x.GetCurrentTime()).Returns(() => _currentTime);
                mockSut.Setup(x => x.GetDueDate(It.IsAny<EventTaskAssignmentModel>())).Returns(() => _dueDate);
                eventTaskAssignment = new EventTaskAssignmentModel
                {
                    AssignmentType = (int)ReminderTypes.Task,
                    LastReminderSent = DateTime.MinValue
                };
                _sut = mockSut.Object;
            }

            [Test]
            public void SendsReminderIfWithinTime()
            {
                _dueDate = _currentTime.AddMinutes((int) ReminderTimes.Task - 1);

                var result = _sut.ShouldSendOneTimeReminder(eventTaskAssignment);

                Assert.That(result, Is.True, "Flags to send reminder if task due date is within window");
            }

            [Test]
            public void DoesNotSendReminderIfPastDueDate()
            {
                _dueDate = _currentTime.AddMinutes(-1);

                var result = _sut.ShouldSendReminder(eventTaskAssignment);

                Assert.That(result, Is.False, "Does not send task reminder if due date is passed");
            }

            [Test]
            public void DoesNotSendReminderIfTooEarlyForWindow()
            {
                _dueDate = _currentTime.AddMinutes(ReminderService.TaskReminderTime + 1);

                var result = _sut.ShouldSendReminder(eventTaskAssignment);

                Assert.That(result, Is.False, "Does not send task reminder if due date is not yet within window");
            }

            [Test]
            public void SendsReminderIfExactlyAtWindow()
            {
                _dueDate = _currentTime.AddMinutes(ReminderService.TaskReminderTime);

                var result = _sut.ShouldSendReminder(eventTaskAssignment);

                Assert.That(result, Is.True, "Flags to send reminder if task due date exactly at window");
            }

            [Test]
            public void DoesNotSendReminderIfAlreadySent()
            {
                _dueDate = _currentTime.AddMinutes((int)ReminderTimes.Task - 1);
                eventTaskAssignment = new EventTaskAssignmentModel()
                {
                    AssignmentType = (int) ReminderTypes.Task,
                    LastReminderSent = DateTime.Now.AddMinutes(-5)
                };
                var result = _sut.ShouldSendOneTimeReminder(eventTaskAssignment);

                Assert.That(result, Is.False, "Does not send reminder if already sent");
            }
        }

        class ProcessRemindersTest
        {
            private ReminderService _sut;
            private Mock<ReminderService> _mockSut;
            private DateTime _currentTime;
            [SetUp]
            public void SetUp()
            {
                _currentTime = DateTime.Parse("October 29, 2019 16:30:00");
                var provider = new ServiceCollection().BuildServiceProvider();
                _mockSut = new Mock<ReminderService>(provider) { CallBase = true };
                _mockSut.Setup(x => x.InitServices(It.IsAny<IServiceProvider>()));
                _mockSut.Setup(x => x.CreateReminderServiceLoggerConfiguration()).Returns(new LoggerConfiguration().CreateLogger);
                _mockSut.Setup(x => x.GetCurrentTime()).Returns(() => _currentTime);
                _mockSut.Setup(x => x.GetListOfAssignments(It.IsAny<ReminderTypes>())).Returns(new List<EventTaskAssignmentModel>());

            }

            [Test]
            public void OnlyFiresTasksWhenOutsideOtherWindows()
            {
                _currentTime = DateTime.Parse($"October 29, 2019 {ReminderTimes.Project - 5}:00:00");

                _mockSut.Object.FireRemindersByAssignmentType();

                _mockSut.Verify(x => x.ProcessReminders(ReminderTypes.Task), Times.Once);
                _mockSut.Verify(x => x.ProcessReminders(ReminderTypes.Project), Times.Never);
            }

            [Test]
            public void FiresTaskAndProjectInWindow()
            {
                _currentTime = DateTime.Parse($"October 29, 2019 {(int)ReminderTimes.Project}:00:00");

                _mockSut.Object.FireRemindersByAssignmentType();

                _mockSut.Verify(x => x.ProcessReminders(ReminderTypes.Task), Times.Once);
                _mockSut.Verify(x => x.ProcessReminders(ReminderTypes.Project), Times.Once);
            }
        }

        class GetDueDates
        {
            private ReminderService _sut;
            private Mock<ReminderService> _mockSut;
            private EventTaskAssignmentModel _eventTaskAssignment;
            private DateTime _taskStartTime;
            private DateTime _projectDueDate;

            [SetUp]
            public void SetUp()
            {
                _taskStartTime = DateTime.Parse("October 29, 2016 16:30:00");
                _projectDueDate = DateTime.Parse("August 13, 2018 7:15:00");

                var provider = new ServiceCollection().BuildServiceProvider();
                _mockSut = new Mock<ReminderService>(provider) { CallBase = true };
                _mockSut.Setup(x => x.InitServices(It.IsAny<IServiceProvider>()));
                _mockSut.Setup(x => x.CreateReminderServiceLoggerConfiguration()).Returns(new LoggerConfiguration().CreateLogger);
                _sut = _mockSut.Object;

                _eventTaskAssignment = new EventTaskAssignmentModel
                {
                    TaskStartTime = _taskStartTime,
                    ProjectDueDate = _projectDueDate
                };

            }

            [Test]
            public void GetsShortTermDueDate()
            {
                _eventTaskAssignment.AssignmentType = (int) ReminderTypes.Task;

                var result = _sut.GetDueDate(_eventTaskAssignment);
                var expected = _taskStartTime;
                
                Assert.That(result, Is.EqualTo(expected), "Gets correct one time reminder due date");
            }

            [Test]
            public void GetsLongTermDueDate()
            {
                _eventTaskAssignment.AssignmentType = (int)ReminderTypes.Project;
                
                var result = _sut.GetDueDate(_eventTaskAssignment);
                var expected = _projectDueDate;

                Assert.That(result, Is.EqualTo(expected), "Gets correct recurring reminder due date");
            }

            [Test]
            public void ThrowsErrorForInvalidTypes()
            {
                _eventTaskAssignment.AssignmentType = -8675309;

                var ex = Assert.Throws<ArgumentException>(() => _sut.GetDueDate(_eventTaskAssignment));
                Assert.That(ex.Message, Is.EqualTo("Assignment does not have a valid type"));
            }
        }
    }
}
