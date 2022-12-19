using System.Text;
using System.Text.Json;
using DiscordBot.Extensions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.EntityFrameworkCore;
using NATS.Client;
using Serilog;
using UncoreMetrics.Data;
using UncoreMetrics.Data.Discord;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DiscordBot.Helpers
{
    public class QueueProcessor
    {
        private static IServiceScopeFactory _scopeFactory;
        private static DiscordShardedClient _client;
        private static UncoreDiscordBotConfiguration _config;
        private static ILogger _logger;

        public static void Load(DiscordShardedClient client, UncoreDiscordBotConfiguration config,
            IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _logger = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<ILogger<QueueProcessor>>();
            _client = client;
            _config = config;
            if (string.IsNullOrWhiteSpace(_config.NATSConnectionURL))
            {
                _logger.LogInformation("NATS URL not setup or blank, aborting Queue Load");
                return;
            }

            var cf = new ConnectionFactory();
            var natsConnection = cf.CreateConnection(GetOpts());

            natsConnection.SubscribeAsync("ServerUpdate", (obj, args) =>
            {
                try
                {
                    var DATA = Encoding.UTF8.GetString(args.Message.Data);
                    var updateInfo = JsonSerializer.Deserialize<ServerUpdateNATs>(DATA);
                    if (updateInfo.ServersDown != null && updateInfo.ServersDown.Any())
                    {
                        DealWithServerUpdate(updateInfo.ServersDown, "Down");
                    }

                    if (updateInfo.ServersUp != null && updateInfo.ServersUp.Any())
                    {
                        DealWithServerUpdate(updateInfo.ServersUp, "Up");
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error with Discordhook");
                }
            });
            _logger.LogInformation($"NATS Enabled, Connection State: {natsConnection.State}");
        }

        public static async Task DealWithServerUpdate(List<Guid> servers, string status)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var serverContext = scope.ServiceProvider.GetRequiredService<ServersContext>();
                var discordContext = scope.ServiceProvider.GetRequiredService<DiscordContext>();

                var getAnyAttachedLinks = await discordContext.Links
                    .Where(link => servers.Contains(link.GameServerID) && link.Enabled).ToListAsync();

                var groupByServer = getAnyAttachedLinks.GroupBy(link => link.GameServerID);
                var getDistinctServerIds = groupByServer.Select(group => group.Key).Distinct().ToArray();

                var getServerInfo = await serverContext.Servers
                    .Where(server => getDistinctServerIds.Contains(server.ServerID)).AsNoTracking().ToListAsync();
                var tasks = new List<Task>(getAnyAttachedLinks.Count);
                foreach (var grouping in groupByServer)
                {
                    var tryGetServerInfo = getServerInfo.FirstOrDefault(server => server.ServerID == grouping.Key);
                    if (tryGetServerInfo != null)
                    {
                        foreach (var link in grouping)
                        {
                            var task = DealWithServerUpdate(link, tryGetServerInfo, status);
                            tasks.Add(task);
                        }
                    }
                }

                await Task.WhenAll(tasks);
                // Save any voided links
                await discordContext.SaveChangesAsync();
                _logger.LogInformation($"Processed {servers.Count} updated servers of status {status}, {tasks.Count} were attached to users and awaited.");
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Exception with dealing with server update");
            }
        }

        public static async Task DealWithServerUpdate(DiscordChannelLink link, Server server, string newStatus)
        {
            try
            {
                var grabDiscord = await _client
                    .GetShard(link.ServerID)
                    .GetGuildAsync(link.ServerID);
                var getMember = (await _client.ShardClients.FirstOrDefault().Value.GetUserAsync(link.UserID))
                    .CreateDiscordMember();

                if (grabDiscord == null)
                {
                    // Void Link
                    Log.Logger.Information(
                        $"User's {link.UserID}'s link has been voided the discord they specified is gone.");
                    await RemoveLink(link, getMember,
                        "We can no longer find the Discord you specified. Most likely the bot was removed from it :(.");
                    return;
                }


                var grabChannel = grabDiscord.GetChannel(link.ChannelID);
                if (grabChannel == null)
                {
                    // Void link
                    Log.Logger.Information(
                        $"User's {link.UserID}'s link has been voided the channel they specified is gone.");
                    await RemoveLink(link, getMember,
                        "We can no longer find the Discord Channel you specified. Most likely the channel was deleted :(.");
                    return;
                }

                var mentionString = getMember.Mention;
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"New Server Status Update Event for {server.Name}",
                    Description = (string.IsNullOrWhiteSpace(mentionString) ? "" : $"{mentionString} - ") +
                                  $"{server.Name} [{server.Address}:{server.Port}] is now {newStatus}.\n We will continue to monitor this server once every few minutes and notify you of any changes.",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text =
                            $"Use /unlinkchannel {link.GameServerID} to disable these."
                    },
                    Timestamp = DateTimeOffset.UtcNow,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = "Uncore"
                    }
                };

                try
                {
                    await grabChannel.SendMessageAsync(embed);
                }
                catch (UnauthorizedException unauthorized)
                {
                    Log.Logger.Information(
                        $"User {link.UserID}'s link has been voided since we  got an unauthorized exception: {unauthorized}");
                    await RemoveLink(link, getMember,
                        $"We do not have the Send Messages Permission in the Channel ({grabChannel.Name}) you told us to send alerts to :(.");
                }
                catch (NotFoundException notfound)
                {
                    Log.Logger.Information(
                        $"User {link.UserID}'s link has been voided since we got an Not Found exception: {notfound}");
                    await RemoveLink(link, getMember,
                        "The channel was deleted, or is missing :(.");
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Exception with dealing with server update");
            }
        }

        public static async Task RemoveLink(DiscordChannelLink link, DiscordMember memberInfo, string reason)
        {
            link.Enabled = false;
            link.LastChanged = DateTime.UtcNow;
            link.LastStatus = reason;
            // Commited later.
            if (memberInfo != null)
            {
                await memberInfo.SendMessageAsync(
                    $"We will no longer be forwarding status alerts for {link.GameServerID} to your specified server due to a permissions error. Reasoning: {reason}.");
            }
        }

        private static Options GetOpts()
        {
            var opts = ConnectionFactory.GetDefaultOptions();
            opts.Url = _config.NATSConnectionURL;
            opts.AllowReconnect = true;
            opts.MaxReconnect = Options.ReconnectForever;
            return opts;
        }
    }
}