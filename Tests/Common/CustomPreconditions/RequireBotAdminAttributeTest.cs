using System;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Moq;
using NUnit.Framework;
using PestoBot.Common.CustomPreconditions;
using PestoBot.Services;

namespace PestoBot.Tests.Common.CustomPreconditions
{
    class RequireBotAdminAttributeTest
    {
        private Mock<RequireBotAdminAttribute> _mockSut;
        private IServiceProvider _provider;
        private bool _isGuildUser;
        private bool _userIsBotOwner;
        private bool _hasAdminRole;
        private CommandContext _cmdContext;
        private List<string> _roles;

        private SocketGuildUser _gUser;
        private CommandInfo _commandInfo;

        [SetUp]
        public void SetUp()
        {
            _roles = new List<string>();

            _commandInfo = (CommandInfo)System.Runtime.Serialization.FormatterServices
                .GetUninitializedObject(typeof(CommandInfo));
            _gUser = (SocketGuildUser)System.Runtime.Serialization.FormatterServices
                .GetUninitializedObject(typeof(SocketGuildUser));

            var client = new Mock<IDiscordClient>().Object;
            var msg = new Mock<IUserMessage>().Object;
            _cmdContext = new Mock<CommandContext>(client, msg).Object;

            _provider = new Mock<IServiceProvider>().Object;

            _mockSut = new Mock<RequireBotAdminAttribute> { CallBase = true };
            _mockSut.Setup(x => x.GetConfigService(
                It.IsAny<IServiceProvider>())).Returns(ConfigService.BuildConfig());
            _mockSut.Setup(x => x.ContextUserIsGuildUser(
                It.IsAny<ICommandContext>())).Returns(() => _isGuildUser);
            _mockSut.Setup(x => x.GetRolesFromGuildUser(
                It.IsAny<SocketGuildUser>())).Returns(() => _roles);
            _mockSut.Setup(x => x.GetContextUserAsGuildUser(
                It.IsAny<CommandContext>())).Returns(() => _gUser);
            _mockSut.Setup(x => x.IsBotOwner(
                    It.IsAny<CommandContext>(), It.IsAny<CommandInfo>(), It.IsAny<IServiceProvider>())).Returns(() => _userIsBotOwner);
        }

        [Test]
        public void GetBotAdminRoles()
        {
            var result = _mockSut.Object.GetAdminRoles(_provider);

            Assert.That(result.Count, Is.EqualTo(2), "Bot admin role list is has correct number of roles");
            Assert.That(result.Contains("PestoAdmin"), "Admin roles contains PestoAdmin");
        }

        [Test]
        public void DoesNotGiveAdminRoleWithoutCorrectRole()
        {
            _roles.AddRange(new []{"Some role 1", "foo", "bar"});

            var result = _mockSut.Object.HasAdminRole(_provider, _roles);

            Assert.That(result, Is.False);
        }

        [Test]
        public void FindsAdminRole()
        {
            _roles.AddRange(new[] { "Some role 1", "foo", "bar", "PestoAdmin" });

            var result = _mockSut.Object.HasAdminRole(_provider, _roles);

            Assert.That(result, Is.True);
        }


        [Test]
        public void ReturnsErrorForNonGuildUser()
        {
            _isGuildUser = false;

            var result = _mockSut.Object.CheckPermissionsAsync(_cmdContext, _commandInfo, _provider).Result;

            Assert.That(result.IsSuccess, Is.False, "Result is not success if user is not guild user");
            Assert.That(result.Error, Is.EqualTo(CommandError.UnmetPrecondition));
            Assert.That(result.ErrorReason, Is.EqualTo(RequireBotAdminAttribute.PermissionError));
        }

        [Test]
        public void ReturnsSuccessIfNoPermissionButIsBotOwner()
        {
            _mockSut.Setup(x => x.HasAdminRole(
                It.IsAny<IServiceProvider>(), It.IsAny<List<string>>())).Returns(() => _hasAdminRole);

            _isGuildUser = true;
            _hasAdminRole = false;
            _userIsBotOwner = true;

            var result = _mockSut.Object.CheckPermissionsAsync(_cmdContext, _commandInfo, _provider).Result;

            Assert.That(result.IsSuccess, Is.True, "Result is success if user is bot admin");
            Assert.That(result.Error, Is.Null);
            Assert.That(result.ErrorReason, Is.Null);
        }

        [Test]
        public void ReturnsSuccessIfHasPermissionButIsNotBotOwner()
        {
            _mockSut.Setup(x => x.HasAdminRole(
                It.IsAny<IServiceProvider>(), It.IsAny<List<string>>())).Returns(() => _hasAdminRole);

            _isGuildUser = true;
            _hasAdminRole = true;
            _userIsBotOwner = false;

            var result = _mockSut.Object.CheckPermissionsAsync(_cmdContext, _commandInfo, _provider).Result;

            Assert.That(result.IsSuccess, Is.True, "Result is success if user has correct role");
            Assert.That(result.Error, Is.Null);
            Assert.That(result.ErrorReason, Is.Null);
        }

    }
}
