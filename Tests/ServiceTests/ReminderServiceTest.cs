using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using Moq;
using NUnit.Framework;
using PestoBot.Common;
using PestoBot.Database.Models.Common;
using PestoBot.Database.Models.SpeedrunEvent;
using PestoBot.Services;

namespace PestoBot.Tests.ServiceTests
{
    class ReminderServiceTest
    {
        class ShouldSendTaskReminderTest
        {
            private ReminderService _sut;
            private DateTime _currentTime;
            private DateTime _dueDate;
            private ReminderModel reminder;

            [SetUp]
            public void SetUp()
            {
                _currentTime = DateTime.Parse("October 29, 2019 16:30:00");
                var mockSut = new Mock<ReminderService>() { CallBase = true };
                mockSut.Setup(x => x.GetCurrentTime()).Returns(() => _currentTime);
                mockSut.Setup(x => x.GetDueDate(It.IsAny<ReminderModel>())).Returns(() => _dueDate);
                reminder = new ReminderModel
                {
                    Type = (int)ReminderTypes.Task,
                    LastSent = DateTime.MinValue
                };
                _sut = mockSut.Object;
            }

            [Test]
            public void SendsReminderIfWithinTime()
            {
                _dueDate = _currentTime.AddMinutes((int) ReminderTimes.Task - 1);

                var result = _sut.ShouldSendTaskReminder(reminder);

                Assert.That(result, Is.True, "Flags to send reminder if task due date is within window");
            }

            [Test]
            public void DoesNotSendReminderIfPastDueDate()
            {
                _dueDate = _currentTime.AddMinutes(-1);

                var result = _sut.ShouldSendReminder(reminder);

                Assert.That(result, Is.False, "Does not send task reminder if due date is passed");
            }

            [Test]
            public void DoesNotSendReminderIfTooEarlyForWindow()
            {
                _dueDate = _currentTime.AddMinutes(ReminderService.TaskReminderTime + 1);

                var result = _sut.ShouldSendReminder(reminder);

                Assert.That(result, Is.False, "Does not send task reminder if due date is not yet within window");
            }

            [Test]
            public void SendsReminderIfExactlyAtWindow()
            {
                _dueDate = _currentTime.AddMinutes(ReminderService.TaskReminderTime);

                var result = _sut.ShouldSendReminder(reminder);

                Assert.That(result, Is.True, "Flags to send reminder if task due date exactly at window");
            }

            [Test]
            public void DoesNotSendReminderIfAlreadySent()
            {
                _dueDate = _currentTime.AddMinutes((int)ReminderTimes.Task - 1);
                reminder = new ReminderModel()
                {
                    Type = (int) ReminderTypes.Task,
                    LastSent = DateTime.Now.AddMinutes(-5)
                };
                var result = _sut.ShouldSendTaskReminder(reminder);

                Assert.That(result, Is.False, "Does not send reminder if already sent");
            }
        }

        class ProcessesTaskReminders
        {
            private ReminderService _sut;
            private Mock<ReminderService> _mockSut;
            private DateTime _currentTime;
            [SetUp]
            public void SetUp()
            {
                _currentTime = DateTime.Parse("October 29, 2019 16:30:00");
                _mockSut = new Mock<ReminderService>() {CallBase = true};
                _mockSut.Setup(x => x.GetCurrentTime()).Returns(() => _currentTime);

                _sut = _mockSut.Object;
            }

            [Test]
            public void OnlyFiresTasksWhenOutsideOtherWindows()
            {
                _currentTime = DateTime.Parse($"October 29, 2019 {ReminderTimes.Project - 5}:00:00");

                _mockSut.Object.FireRemindersByType();

                _mockSut.Verify(x => x.ProcessReminders(ReminderTypes.Task), Times.Once);
                _mockSut.Verify(x => x.ProcessReminders(ReminderTypes.Project), Times.Never);
            }

            [Test]
            public void FiresTaskAndProjectInWindow()
            {
                _currentTime = DateTime.Parse($"October 29, 2019 {(int)ReminderTimes.Project}:00:00");

                _mockSut.Object.FireRemindersByType();

                _mockSut.Verify(x => x.ProcessReminders(ReminderTypes.Task), Times.Once);
                _mockSut.Verify(x => x.ProcessReminders(ReminderTypes.Project), Times.Once);
            }
        }

        class GetDueDates
        {
            private ReminderService _sut;
            private Mock<ReminderService> _mockSut;
            private DateTime _currentTime;
            private IPestoModel _reminderAssignment;

            [SetUp]
            public void SetUp()
            {
                _currentTime = DateTime.Parse("October 29, 2019 16:30:00");
                _mockSut = new Mock<ReminderService>() { CallBase = true };
                _mockSut.Setup(x => x.GetCurrentTime()).Returns(() => _currentTime);
                _mockSut.Setup(x => x.GetAssignmentForReminder(It.IsAny<ReminderModel>()))
                        .Returns(() => _reminderAssignment);

                _sut = _mockSut.Object;
            }
            
            [Test]
            public void GetsShortTermDueDate()
            {
                _reminderAssignment = new MarathonTaskAssignmentModel
                {
                    TaskStartTime = _currentTime.AddMinutes(29)
                };

                var reminder = new ReminderModel
                {
                    Type = (int) ReminderTypes.Task
                };

                var result = _sut.GetShortTermDueDate(reminder);
                var expected = ((MarathonTaskAssignmentModel) _reminderAssignment).TaskStartTime;
                Assert.That(result, Is.EqualTo(expected), "Gets correct short term due date");
            }

            [Test]
            public void GetsLongTermDueDate()
            {
                _reminderAssignment = new MarathonProjectAssignmentModel();

                var dueDate = _currentTime.AddDays(1);
                var projectModel = new MarathonProjectModel()
                {
                    DueDate = dueDate
                };

                var reminder = new ReminderModel
                {
                    Type = (int) ReminderTypes.Project
                };

                _mockSut.Setup(x => x.GetProjectForAssignment(It.IsAny<MarathonProjectAssignmentModel>()))
                    .Returns(projectModel);
                var localSut = _mockSut.Object;
                var result = localSut.GetLongTermDueDate(reminder);
                
                Assert.That(result, Is.EqualTo(dueDate));
            }
        }
    }
}
