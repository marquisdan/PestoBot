using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using Moq;
using NUnit.Framework;
using PestoBot.Common;
using PestoBot.Services;

namespace PestoBot.Tests.ServiceTests
{
    class ReminderServiceTest
    {
        class ShouldSendTaskReminderTest
        {
            private ReminderService _sut;
            private DateTime _currentTime;

            [SetUp]
            public void SetUp()
            {
                _currentTime = DateTime.Parse("October 29, 2019 16:30:00");
                var mockSut = new Mock<ReminderService>() { CallBase = true };
                mockSut.Setup(x => x.GetCurrentTime()).Returns(_currentTime);
                _sut = mockSut.Object;
            }

            [Test]
            public void SendsReminderIfWithinTime()
            {
                var reminderDueTime = _currentTime.AddMinutes(ReminderService.TaskReminderTime -1);

                var result = _sut.ShouldSendTaskReminder(reminderDueTime);

                Assert.That(result, Is.True, "Flags to send reminder if task due date is within window");
            }

            [Test]
            public void DoesNotSendReminderIfPastDueDate()
            {
                var reminderDueTime = _currentTime.AddMinutes(-1);

                var result = _sut.ShouldSendTaskReminder(reminderDueTime);

                Assert.That(result, Is.False, "Does not send task reminder if due date is passed");
            }

            [Test]
            public void DoesNotSendReminderIfTooEarlyForWindow()
            {
                var reminderDueTime = _currentTime.AddMinutes(ReminderService.TaskReminderTime + 1);

                var result = _sut.ShouldSendTaskReminder(reminderDueTime);

                Assert.That(result, Is.False, "Does not send task reminder if due date is not yet within window");
            }

            [Test]
            public void SendsReminderIfExactlyAtWindow()
            {
                var reminderDueTime = _currentTime.AddMinutes(ReminderService.TaskReminderTime);

                var result = _sut.ShouldSendTaskReminder(reminderDueTime);

                Assert.That(result, Is.True, "Flags to send reminder if task due date exactly at window");
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
    }
}
