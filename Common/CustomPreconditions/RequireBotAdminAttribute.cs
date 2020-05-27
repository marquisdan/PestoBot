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
        protected internal static string PermissionError = "You do not have permission to run this command.";

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (ContextUserIsGuildUser(context))
            {
                var gUser = GetContextUserAsGuildUser(context);
                var roles = GetRolesFromGuildUser(gUser);
                // If this command was executed by a user with the appropriate role, OR is bot owner return a success
                if (HasAdminRole(services, roles) || IsBotOwner(context, command, services))
                    return Task.FromResult(PreconditionResult.FromSuccess());
                // Since it wasn't, fail
                return Task.FromResult(PreconditionResult.FromError("You do not have permission to run this command."));
            }

            return Task.FromResult(PreconditionResult.FromError(PermissionError));
        }

        protected internal virtual bool HasAdminRole(IServiceProvider services, List<string> roles)
        {
            return roles.Intersect(GetAdminRoles(services)).Any();
        }

        protected internal virtual bool IsBotOwner(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            return new RequireOwnerAttribute().CheckPermissionsAsync(context, command, services).Result.IsSuccess;
        }

        protected internal virtual List<string> GetRolesFromGuildUser(SocketGuildUser gUser)
        {
            return (from r in gUser.Roles select r.Name).ToList();
        }

        protected internal virtual SocketGuildUser GetContextUserAsGuildUser(ICommandContext context)
        {
            return context.User as SocketGuildUser;
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
