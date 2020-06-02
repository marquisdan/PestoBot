using System;
using System.Linq;
using System.Reflection;
using Discord.Commands;
using Moq;
using NUnit.Framework;
using PestoBot.Modules.EmbedBuilders;

namespace PestoBot.Tests.Common
{
    class ModuleInfoUtilsTest
    {
        private Assembly pestoAssembly;
        //private 
        private ModuleInfoUtils sut;

        [SetUp]
        public void SetUp()
        {
            pestoAssembly = AppDomain.CurrentDomain.GetAssemblies().First(x => x.FullName.Contains("PestoBot"));
            var mockSut = new Mock<ModuleInfoUtils> {CallBase = true};
            mockSut.Setup(x => x.GetEntryAssembly()).Returns(pestoAssembly);
            sut = mockSut.Object;
        }

        [Test]
        public void GetsModulesOnly()
        {
            var result = sut.GetAllModules();
            var nonModules = result.Where(typeInfo => !typeof(ModuleBase).IsAssignableFrom(typeInfo));

            Assert.That(result, Is.Not.Empty);
            Assert.That(nonModules, Is.Empty);
        }
    }
}
