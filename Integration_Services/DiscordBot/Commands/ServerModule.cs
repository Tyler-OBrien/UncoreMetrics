using System.Diagnostics.CodeAnalysis;
using System.Net;
using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using UncoreMetrics.Data;

namespace DiscordBot.Commands
{
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class ServerModule : ApplicationCommandModule
    {
        [NotNull] public ServersContext ServersContext { get; set; }


        [Command("server")]
        [RequireGuild]
        [Cooldown(2, 5, CooldownBucketType.User)]
        [Description("Search for Servers via Uncore")]
        [SlashCommand("server", "This command is used to find server info")]
        public Task FindServer(InteractionContext ctx,
            [Option("Server", "Name, IP or GUID to search for server with")]
            string search,
            [Option("includeDead", "Include Dead Servers")]
            bool includeDead = false)
        {
            return SearchServer(ctx, search, includeDead);
        }


        [Command("search")]
        [RequireGuild]
        [Cooldown(2, 5, CooldownBucketType.User)]
        [Description("Search for Servers via Uncore")]
        [SlashCommand("search", "This command is used to search for servers.")]
        public async Task SearchServer(InteractionContext ctx,
            [Option("Server", "Name, IP or GUID to search for server with")] string search,
            [Option("includeDead", "Include Dead Servers")] bool includeDead = false)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            _ = Task.Run(async () =>
            {
                IPAddress searchAddress;
                IPAddress.TryParse(search, out searchAddress);
                if (searchAddress == null)
                {
                    searchAddress = IPAddress.None;
                }

                Guid searchGUID;
                Guid.TryParse(search, out searchGUID);
                if (searchGUID == null)
                {
                    searchGUID = Guid.Empty;
                }

                List<Server> foundServers;

                if (includeDead == false)
                {
                    foundServers = await ServersContext.Servers
                        .Where(server =>
                            server.SearchVector.Matches(search) || server.Address == searchAddress ||
                            server.ServerID == searchGUID)
                        .Where(server => server.ServerDead == includeDead).OrderByDescending(server => server.Players)
                        .Take(25).AsNoTracking().ToListAsync();
                }
                else
                {
                    foundServers = await ServersContext.Servers
                        .Where(server =>
                            server.SearchVector.Matches(search) || server.Address == searchAddress ||
                            server.ServerID == searchGUID)
                        .OrderByDescending(server => server.Players).Take(25).AsNoTracking().ToListAsync();
                }

                if (foundServers.Any() == false)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent(
                            "Found no servers. Looked for matches on Name, IP Address, or Server GUID. Use https://uncore.app for better search/filtering."));
                    return;
                }

                if (foundServers.Count == 1)
                {
                    var server = foundServers[0];
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .AddEmbed(GenerateEmbedFromServer(server)));
                    return;
                }

                // Create the options for the user to pick
                var options = new List<DiscordSelectComponentOption>();
                foreach (var foundServer in foundServers)
                {
                    options.Add(new DiscordSelectComponentOption($"{foundServer.Name}", foundServer.ServerID.ToString(),
                        $"{foundServer.Name} on {foundServer.Address} for {foundServer.Game}"));
                }

                // Make the dropdown
                var dropdown = new DiscordSelectComponent("dropdown", "Pick Server", options);

                var msg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddComponents(dropdown)
                    .WithContent($"Found {foundServers.Count}. Pick one to see info. You have one minute."));

                var select = await msg.WaitForSelectAsync(ctx.User, dropdown.CustomId, TimeSpan.FromMinutes(1));

                if (select.TimedOut)
                {
                    return;
                }

                var selectedValue = select.Result.Values.FirstOrDefault();
                var tryGetServer = foundServers.FirstOrDefault(server =>
                    server.ServerID.ToString().Equals(selectedValue, StringComparison.OrdinalIgnoreCase));
                if (tryGetServer != null)
                {
                    await select.Result.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                        new DiscordInteractionResponseBuilder()
                            .AddEmbed(GenerateEmbedFromServer(tryGetServer)));
                }
                else
                {
                    await select.Result.Interaction.CreateResponseAsync(
                        InteractionResponseType.UpdateMessage,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("Something went wrong with processing the response"));
                }
            });
        }

        public DiscordEmbed GenerateEmbedFromServer(Server server)
        {
            return new DiscordEmbedBuilder
            {
                Title = $"{server.Name} on {server.Game}",
                Description =
                    $"ID: {server.ServerID}\nServer Address:{server.IpAddress}:{server.Port}\nQuery Address:{server.IpAddress}:{server.QueryPort}\nPlayers: {server.Players}/{server.MaxPlayers}\n" +
                    $"Status: {(server.IsOnline ? "Online" : "Offline")}\nCountry:{server.Country}\nTimezone: {server.Timezone}\nLast Check: {server.LastCheck.Humanize(true, DateTime.UtcNow)}\n" +
                    $"Found At UTC: {server.FoundAt.ToString("F")}\nNext Check: {server.NextCheck.Humanize(true)}\nISP: {server.ISP}\nASN: {server.ASN}",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text =
                        $"To Get Notifications on this server's status, you can do /linkchannel <channel> {server.ServerID}"
                },
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = "Uncore"
                },
                Timestamp = DateTimeOffset.UtcNow
            };
        }
    }
}