using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Discord.Commands;

namespace PestoBot.Modules.EmbedBuilders
{
    public class ModuleInfoUtils
    {
        public List<TypeInfo> GetAllModules()
        {
            List<TypeInfo> result = new List<TypeInfo>();
            foreach (TypeInfo definedType in GetEntryAssembly().DefinedTypes)
            {
                if (definedType.IsPublic || definedType.IsNestedPublic)
                {
                    if (IsValidModule(definedType)) { result.Add(definedType);}
                }
            }

            return result;
        }

        protected internal virtual Assembly GetEntryAssembly()
        {
            return Assembly.GetEntryAssembly();
        }

        public string GetCommands(MethodInfo methodInfo)
        {
            var a = methodInfo.GetCustomAttributes(typeof(CommandAttribute));
            var commands = a.Select(x => x as CommandAttribute);
            return commands.Select(x => x.Text).First();
        }

        public List<MethodInfo> GetFirstPublicMethods(TypeInfo typeInfo, int numMethods)
        {
            return GetPublicMethods(typeInfo).Take(numMethods).ToList();
        }

        public List<MethodInfo> GetPublicMethods(TypeInfo typeInfo)
        {
            return typeInfo.AsType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();
        }

        internal static bool IsValidModule(TypeInfo typeInfo)
        {
            var baseType = typeof(ModuleBase);
            return (typeof(ModuleBase).IsAssignableFrom(typeInfo) || typeof(ModuleBase<SocketCommandContext>).IsAssignableFrom(typeInfo)) && !typeInfo.IsAbstract;
        }
    }
}