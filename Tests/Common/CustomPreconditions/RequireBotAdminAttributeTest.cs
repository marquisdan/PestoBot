using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using PestoBot.Common.CustomPreconditions;
using PestoBot.Services;

namespace PestoBot.Tests.Common.CustomPreconditions
{
    class RequireBotAdminAttributeTest
    {

        [Test]
        public void GetBotAdminRoles()
        {
            var mockSut = new Mock<RequireBotAdminAttribute>() {CallBase = true};
            mockSut.Setup(x => x.GetConfigService(
                                It.IsAny<IServiceProvider>())).Returns(ConfigService.BuildConfig());

            var provider = new Mock<IServiceProvider>().Object;

            var result = mockSut.Object.GetAdminRoles(provider);

            Assert.That(result.Count, Is.EqualTo(2), "Bot admin role list is has correct number of roles");
            Assert.That(result.Contains("PestoAdmin"), "Admin roles contains PestoAdmin");
        }

    }
}
