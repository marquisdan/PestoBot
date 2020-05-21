using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PestoBot.Common.CustomPreconditions
{
    public class RequireBotAdminAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (ContextUserIsGuildUser(context))
            {
                var gUser = context.User as SocketGuildUser;
                var roles = (from r in gUser.Roles select r.Name);
                // If this command was executed by a user with the appropriate role, OR is bot owner return a success
                if (roles.Intersect(GetAdminRoles(services)).Any() || 
                    new RequireOwnerAttribute().CheckPermissionsAsync(context, command, services).Result.IsSuccess)
                    return Task.FromResult(PreconditionResult.FromSuccess());
                // Since it wasn't, fail
                return Task.FromResult(PreconditionResult.FromError("You do not have permission to run this command."));
            }

            return Task.FromResult(PreconditionResult.FromError("You do not have permission to run this command."));
        }

        protected internal virtual bool ContextUserIsGuildUser(ICommandContext context)
        {
            return context.User is SocketGuildUser user;
        }

        protected internal virtual List<string> GetAdminRoles(IServiceProvider services)
        {
            var config = GetConfigService(services);
            var roleSectionMembers = config.GetSection("BotAdminRoles").GetChildren();

            List<string> AdminRoles = (from c in roleSectionMembers select c.Value).ToList();

            return AdminRoles;
        }

        protected internal virtual IConfiguration GetConfigService(IServiceProvider services)
        {
            return services.GetRequiredService<IConfiguration>();
        }
    }
}
