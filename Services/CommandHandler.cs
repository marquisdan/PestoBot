using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace PestoBot.Services
{
    public class CommandHandler
    {
        private IConfiguration _config;
        private DiscordSocketClient _client;
        private CommandService _commands;
        private ILogger _logger;
        private IServiceProvider _serviceProvider;

        private string CommandPrefix;
        protected internal ulong MWSF;
        protected internal ulong BotSandbox;
        protected internal string EscapedPesto;
        protected internal string EscapedDabesto;

        public CommandHandler(IServiceProvider services)
        {
            // Virtual for mocking in test
            // ReSharper disable VirtualMemberCallInConstructor
            InitServices(services);
            InitFields();
        }

        protected internal virtual void InitFields()
        {
            CommandPrefix = _config["DefaultPrefix"];
            EscapedPesto = _config.GetSection("Emotes").GetSection("EscapedPesto").Value;
            EscapedDabesto = _config.GetSection("Emotes").GetSection("EscapedDabesto").Value;
            MWSF = ulong.Parse(_config.GetSection("SpecialGuilds").GetSection("MWSF").Value);
            BotSandbox = ulong.Parse(_config.GetSection("SpecialGuilds").GetSection("BotSandbox").Value);
        }

        protected internal virtual void InitServices(IServiceProvider services)
        {
            _config = services.GetRequiredService<IConfiguration>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _commands = services.GetRequiredService<CommandService>();
            _logger = services.GetRequiredService<ILogger<CommandHandler>>();
            _serviceProvider = services;
        }

        public virtual async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived & Command Executed events to handlers
            _commands.CommandExecuted += HandleCommandAsync;
            _client.MessageReceived += HandleMessageAsync;

            //Add all public modules that inherit ModuleBase
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        }

        protected internal virtual async Task HandleMessageAsync(SocketMessage messageParam)
        {
            // Don't process system messages
            if (!(messageParam is SocketUserMessage msg))
            {
                return;
            }

            // We don't want the bot to respond to itself or other bots.
            if (IsSelfOrBot(msg)) return;

            //Don't process DMs
            if (IsDM(msg))
            {
                await HandleDms(msg);
                return;
            }

            if (MsgHasPestoContent(msg.Content))
            {
                await HandlePesto(msg);
            }

            // Create a number to track where the prefix ends and the command begins
            int pos = 0;

            //Trigger on prefix OR @mentioning bot
            await PrefixTriggers(msg, pos);
        }

        protected internal virtual bool IsDM(SocketMessage messageParam)
        {
            return messageParam.Channel is IDMChannel;
        }

        private async Task HandleDms(SocketUserMessage msg)
        {
            var context = new SocketCommandContext(_client, msg);
            Log.Information($"DM Received: {context.User.Username}: {context.Message.Content}");
            await context.Channel.SendMessageAsync(
                $"Sorry, {context.User.Username}, {_client.CurrentUser.Username} is not set up for DMs yet");
            await context.Channel.SendMessageAsync($"Contact marquisdan if you have any questions");
        }

        protected internal virtual bool IsSelfOrBot(SocketUserMessage msg)
        {
            //This is separated out for mocking in tests
            return msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot;
        }

        protected internal virtual bool MsgHasPestoContent(string msgContent)
        {
            //This is separated out for mocking in tests
            return msgContent.ToLower().Contains("pesto");
        }

        /// <summary>
        /// Handles all commands that start with prefix or directly @ bot
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        protected internal virtual async Task PrefixTriggers(SocketUserMessage msg, int pos)
        {
            if (msg.HasStringPrefix(CommandPrefix, ref pos) ||
                msg.HasMentionPrefix(_client.CurrentUser, ref pos))
            {
                //Log for debug purposes
                Console.WriteLine($"{msg.Author} : {msg.Content}");

                // Create a WebSocket-based command context based on the message
                var context = new SocketCommandContext(_client, msg);

                // Execute the command with the command context we just
                // created, along with the service provider for precondition checks.
                await _commands.ExecuteAsync(context, pos, _serviceProvider);
            }
        }


        /// <summary>
        /// Handles any pesto that may need to be handled.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        protected internal virtual async Task HandlePesto(SocketUserMessage msg)
        {
            var guildId = GetMessageGuildId(msg);

            if (guildId == MWSF)
                await AddPestoReactions(msg);
            else if (guildId == BotSandbox) await AddTestReactions(msg);
        }

        protected internal virtual ulong GetMessageGuildId(SocketUserMessage msg)
        {
            //This is separated out for mocking in tests
            return ((SocketGuildChannel)msg.Channel).Guild.Id;
        }

        protected internal virtual async Task AddPestoReactions(SocketUserMessage msg)
        {
            //This is separated out for mocking in tests
            if (Emote.TryParse(EscapedPesto, out var pesto) && Emote.TryParse(EscapedDabesto, out var dabesto))
            {
                await msg.AddReactionAsync(pesto);
                await msg.AddReactionAsync(dabesto);
            }
        }

        protected internal virtual async Task AddTestReactions(SocketUserMessage msg)
        {
            //Do nothing for now. Need to add emotes to bot server

            //if (Emote.TryParse(EscapedPesto, out var pesto) && Emote.TryParse(EscapedDabesto, out var dabesto))
            //{
            //    await msg.AddReactionAsync(pesto);
            //    await msg.AddReactionAsync(dabesto);
            //}
        }

        protected virtual async Task HandleCommandAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified)
            {
                Log.Warning($"{context.Guild.Name}: Command failed to execute for [{context.User.Username}] <-> [{result.ErrorReason}]!");
                return;
            }

            if (result.IsSuccess)
            {
                Log.Information($"{context.Guild.Name}: Command [{command.Value.Name}] executed for -> [{context.User.Username}]");
                return;
            }

            //If command was specified but not success, we have encountered an error
            //Log error for user
            Log.Error($"Command [{command.Value.Name}] failed for [{context.User.Username}]: [{result}]");
            await context.Channel.SendMessageAsync($"Sorry, {context.User.Username} something went wrong -> [{result}]!");
        }

    }

}
