using System;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using PestoBot.Services;

namespace PestoBot.Tests.ServiceTests
{
    public class CommandHandlerTests
    {
        #region SetUp
        private CommandHandler _sut;
        private Mock<CommandHandler> _mockSut;
        private Mock<IUserMessage> _mockMsg;
        private SocketMessage _msg;
        private string _msgContent;
        private ulong _guildId;
        private ulong _mwsf;
        private ulong _botSandBox;
        private Microsoft.Extensions.Configuration.IConfiguration _config;

        [SetUp]
        public void SetUp()
        {
            _msgContent = "";
            _guildId = 1; 
            _config = ConfigService.BuildConfig();
            _mwsf = ulong.Parse(_config.GetSection("SpecialGuilds").GetSection("MWSF").Value);
            _botSandBox = ulong.Parse(_config.GetSection("SpecialGuilds").GetSection("BotSandbox").Value);

            _mockMsg = new Mock<IUserMessage>();
            _mockMsg.Setup(x => x.Content).Returns(() => _msgContent);
            _msg = _mockMsg.Object as SocketMessage;

            var provider = new ServiceCollection().BuildServiceProvider();
            _mockSut = new Mock<CommandHandler>(provider) { CallBase = true };
            _mockSut.Setup(x => x.InitFields()).Verifiable();
            _mockSut.Setup(x => x.InitServices(It.IsAny<IServiceProvider>()));
            _mockSut.Setup(x => x.PrefixTriggers(It.IsAny<SocketUserMessage>(), It.IsAny<int>()));
            _mockSut.Setup(x => x.GetMessageGuildId(It.IsAny<SocketUserMessage>())).Returns(() => _guildId);

            _mockSut.Object.MWSF = _mwsf;
            _mockSut.Object.BotSandbox = _botSandBox;
        }
        #endregion

        #region EmoteReactTests
        [Test]
        public void DoesNotAddEmotesIfWrongServer()
        {
            _guildId = 1234;

            _mockSut.Object.HandlePesto(_msg as SocketUserMessage);

            _mockSut.Verify(x => x.AddPestoReactions(It.IsAny<SocketUserMessage>()), Times.Never());
            _mockSut.Verify(x => x.AddTestReactions(It.IsAny<SocketUserMessage>()), Times.Never());
        }

        [Test]
        public void AddsPestoIfMwsfGuild()
        {
            _guildId = _mwsf;

            _mockSut.Object.HandlePesto(_msg as SocketUserMessage);

            _mockSut.Verify(x => x.AddPestoReactions(It.IsAny<SocketUserMessage>()), Times.Exactly(1));
            _mockSut.Verify(x => x.AddTestReactions(It.IsAny<SocketUserMessage>()), Times.Never());
        }

        [Test]
        public void AddsTestReactsOnBotSandboxServer()
        {
            _guildId = _botSandBox;

            _mockSut.Object.HandlePesto(_msg as SocketUserMessage);

            _mockSut.Verify(x => x.AddPestoReactions(It.IsAny<SocketUserMessage>()), Times.Never());
            _mockSut.Verify(x => x.AddTestReactions(It.IsAny<SocketUserMessage>()), Times.Exactly(1));
        }

        #endregion

    }
}