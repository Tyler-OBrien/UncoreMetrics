using System.Text.Json;
using DiscordBot.Commands;
using DiscordBot.Helpers;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Humanizer;
using Humanizer.Localisation;
using Microsoft.Extensions.Options;
using Sentry;
using Sentry.Extensibility;
using Serilog;
using Serilog.Extensions.Logging;

namespace DiscordBot
{
    public class Worker : BackgroundService
    {
        private readonly UncoreDiscordBotConfiguration _configuration;
        private readonly ILogger<Worker> _logger;

        private readonly IServiceScopeFactory _scopeFactory;

        public Worker(ILogger<Worker> logger, IOptions<UncoreDiscordBotConfiguration> configuration,
            IServiceScopeFactory discordBotScope)
        {
            _logger = logger;
            _configuration = configuration.Value;
            _scopeFactory = discordBotScope;
        }

        public static DiscordShardedClient discordClient { get; private set; }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var serviceProvider = scope.ServiceProvider;
            var newFactory = new SerilogLoggerFactory(Log.Logger, true);

            while (!stoppingToken.IsCancellationRequested)
            {
                TaskScheduler.UnobservedTaskException += async (_, e) =>
                    Log.Logger.Error(e.Exception, "Task Scheduler caught an unobserved exception");

                discordClient = new DiscordShardedClient(new DiscordConfiguration
                {
                    Token = _configuration.DiscordToken,
                    TokenType = TokenType.Bot,
                    LoggerFactory = newFactory,
                    LogTimestampFormat = "h:mm:ss ff tt",
                    MinimumLogLevel = LogLevel.Trace,
                    Intents = DiscordIntents.Guilds
                });

                var interactivity = await discordClient.UseInteractivityAsync();


                var slashcommands = await discordClient.UseSlashCommandsAsync(new SlashCommandsConfiguration
                    { Services = serviceProvider });
                foreach (var extension in slashcommands.Select(i => i.Value))
                {
                    extension.RegisterCommands<LinkChannelModule>();
                    extension.RegisterCommands<ServerModule>();
                }

                discordClient.Ready += DiscordClientOnReady;
                discordClient.GuildAvailable += DiscordClientOnGuildAvailable;
                discordClient.ClientErrored += DiscordClientOnClientErrored;
                discordClient.SocketClosed += OnSocketErrored;


                await discordClient.StartAsync();
                QueueProcessor.Load(discordClient, _configuration, _scopeFactory);
                await Task.Delay(-1);
            }
        }


        private static async Task OnSocketErrored(DiscordClient c, SocketCloseEventArgs e)
        {
            if (e.CloseCode is 4014)
            {
                Log.Logger.Error(
                    $"Missing intents! Enable them on the developer dashboard (discord.com/developers/applications/{c.CurrentApplication.Id})");
                Log.Logger.Error(e.CloseMessage);
                Log.Logger.Error(JsonSerializer.Serialize(e));
            }
        }

        private static async Task OnCommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            var Handled = false;
            switch (e.Exception)
            {
                case ChecksFailedException f:
                    await ShowChecksFailedMessage(e, sender.Client, f);
                    Handled = true;
                    break;
                case ArgumentException { Message: "Could not find a suitable overload for the command." }:
                    await SendHelpAsync(sender.Client, e.Command.QualifiedName, e.Context);
                    Handled = true;
                    break;
                case InvalidOperationException
                {
                    Message: "No matching subcommands were found, and this group is not executable."
                }:
                    await SendHelpAsync(sender.Client, e.Command.QualifiedName, e.Context);
                    Handled = true;
                    break;
                case CommandNotFoundException:
                    Handled = true;
                    break;
            }

            if (Handled == false)
            {
                Log.Logger.ForContext(e?.Command?.Module?.ModuleType)
                    .Warning(e.Exception, "Something went wrong!");
            }
            //_logger.LogWarning(e.Exception.InnerException ?? e.Exception , "A command threw an exception! Command: {CommandName}", e.Command.Name);
        }

        private static async Task SendHelpAsync(DiscordClient c, string commandName, CommandContext originalContext)
        {
            var cnext = c.GetCommandsNext();
            var cmd = cnext.RegisteredCommands["help"];
            var ctx = cnext.CreateContext(originalContext.Message, null, cmd, commandName);
            await cnext.ExecuteCommandAsync(ctx);
        }

        private static async Task ShowChecksFailedMessage(CommandErrorEventArgs e, DiscordClient c,
            ChecksFailedException cf)
        {
            var owner = string.Join(", ", c.CurrentApplication.Owners.Select(o => $"{o.Username}#{o.Discriminator}"));
            foreach (var check in cf.FailedChecks)
            {
                var message = check switch
                {
                    RequireOwnerAttribute =>
                        $"My owners is: {owner}. {cf.Context.User.Username}#{cf.Context.User.Discriminator} doesn't look like any of those names!",
                    RequireNsfwAttribute => "Requires NSFW Channel.",
                    CooldownAttribute cd =>
                        $"Command on cooldown. You can use it {cd.MaxUses} time(s) every {cd.Reset.Humanize(2, minUnit: TimeUnit.Second)}!",
                    RequireUserPermissionsAttribute p =>
                        $"You need to have permission to {p.Permissions.Humanize(LetterCasing.Title)} to use this!",
                    RequireDirectMessageAttribute => "This command is only available in DMs.",
                    RequireGuildAttribute => "This command is only available outside of DMS",
                    _ => null
                };

                if (message is not null)
                {
                    await e.Context.RespondAsync(message);
                    return;
                }
            }
        }

        private static Task DiscordClientOnClientErrored(DiscordClient sender, ClientErrorEventArgs e)
        {
            sender.Logger.LogError(e.Exception, $"Exception occured in shard {sender.ShardId}");
            return Task.CompletedTask;
        }

        private static Task DiscordClientOnGuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            sender.Logger.LogInformation($"Guild available: {e.Guild.Name} for shard {sender.ShardId}");
            return Task.CompletedTask;
        }

        private static Task DiscordClientOnReady(DiscordClient sender, ReadyEventArgs e)
        {
            sender.Logger.LogInformation($"Client is ready to process events, shard {sender.ShardId}.");
            return Task.CompletedTask;
        }

        public class SentryEventProcessor : ISentryEventProcessor
        {
            public SentryEvent Process(SentryEvent @event)
            {
                if (@event.Exception.Message.Contains("Connection terminated") &&
                    @event.Exception.Message.Contains("reconnecting"))
                {
                    Log.Logger.Information(@event.Exception, "Ignoring Exception due to filter");
                    return null;
                }

                return @event;
            }
        }
    }
}