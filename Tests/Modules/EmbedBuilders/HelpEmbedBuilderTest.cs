using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using PestoBot.Modules;
using PestoBot.Modules.EmbedBuilders;

namespace PestoBot.Tests.Modules
{
    class HelpEmbedBuilderTest
    {
    //    private ModuleInfoUtils moduleInfoUtils;

    //    [SetUp]
    //    public void SetUp()
    //    {
    //        var pestoAssembly = AppDomain.CurrentDomain.GetAssemblies().First(x => x.FullName.Contains("PestoBot"));
    //        var mockModuleInfoUtils = new Mock<ModuleInfoUtils> {CallBase = true};
    //        mockModuleInfoUtils.Setup(x => x.GetEntryAssembly()).Returns(pestoAssembly);
    //        moduleInfoUtils = mockModuleInfoUtils.Object;
    //    }

    //    [Test]
    //    public void GetsMethodsForEveryModule()
    //    {
    //        var modules = moduleInfoUtils.GetAllModules();
            
    //        foreach (var module in modules)
    //        {
    //            var foo = typeof(HelpEmbedBuilder).MakeGenericType(module.AsType());
    //            dynamic bar = Activator.CreateInstance(foo);
    //            Assert.That(bar.GetPublicMethods(), Is.Not.Empty);
    //        }
    //    }
    }
}
