using System.Diagnostics.CodeAnalysis;
using DiscordBot.Extensions;
using DiscordBot.Helpers;
using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UncoreMetrics.Data;
using UncoreMetrics.Data.Discord;

namespace DiscordBot.Commands
{
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class LinkChannelModule : ApplicationCommandModule
    {
        [NotNull] public ServersContext ServersContext { get; set; }

        [NotNull] public DiscordContext DiscordContext { get; set; }

        [NotNull] public IOptions<UncoreDiscordBotConfiguration> IOptionsConfiguration { get; set; }

        [NotNull] public UncoreDiscordBotConfiguration Configuration => IOptionsConfiguration.Value;


        [SlashCommand("linkchannel", "Link Server Alerts to a channel")]
        [Cooldown(2, 5, CooldownBucketType.User)]
        public async Task LinkChannelCommand(InteractionContext ctx,
            [Option("Channel", "The Discord Channel to link to")] DiscordChannel channel,
            [Option("Server", "Uncore Server ID to get Alerts for")] string server)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            _ = Task.Run(async () =>
            {
                var permissions = ctx.Member.PermissionsIn(channel);
                if (permissions.HasPermission(Permissions.ManageMessages) == false && !ctx.Member.IsAdministrator() &&
                    !ctx.Member.IsOwner)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent(
                            "You need Manage Message Permissions in the specified Discord Channel in order to hook up Server Alerts"));
                    return;
                }

                if (channel.Type != ChannelType.Text)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent(
                            "You need to select a valid text channel."));
                    return;
                }

                var findPermissions = ctx.Guild.CurrentMember.PermissionsIn(channel);
                if (findPermissions.HasPermission(Permissions.SendMessages) == false && !ctx.Member.IsAdministrator() &&
                    !ctx.Member.IsOwner)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent(
                            "The bot needs Send Messages Permission in the channel in order to be able to send out alerts."));
                    return;
                }

                if (Guid.TryParse(server, out var serverGUID) == false)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent(
                            $"Could not parse {server} as a Uncore Server GUID."));
                    return;
                }

                var tryGetServer =
                    await ServersContext.Servers.AsNoTracking().FirstOrDefaultAsync(findServer => findServer.ServerID == serverGUID);
                if (tryGetServer == null)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent(
                            $"Could not find any server with the GUID of {serverGUID}"));
                    return;
                }


                var tryGetLink = await DiscordContext.Links.FirstOrDefaultAsync(link =>
                    link.ChannelID == channel.Id && link.GameServerID == tryGetServer.ServerID && link.Enabled);
                if (tryGetLink != null)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent(
                            "This channel is already hooked up for notifications for this server. You can do /unlinkchannel <ServerID> if you are a Server Admin, or if the link belongs to you."));
                    return;
                }


                var tryGetLinkServer = await DiscordContext.Links.FirstOrDefaultAsync(link =>
                    link.ServerID == channel.GuildId && link.GameServerID == tryGetServer.ServerID && link.Enabled);
                if (tryGetLinkServer != null)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent(
                            "This server is already hooked up for notifications for this server. You can do /unlinkchannel <ServerID> if you are a Server Admin, or if the link belongs to you."));
                    return;
                }

                var tryFindCountsPerUser =
                    await DiscordContext.Links.CountAsync(link => link.UserID == ctx.User.Id && link.Enabled);
                if (tryFindCountsPerUser > Configuration.MaxServerLinksPerUser)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent(
                            $"You can only have a max of {Configuration.MaxServerLinksPerUser} hooked up alerts, Discord-wide."));
                    return;
                }

                var tryFindCountsPerGuild =
                    await DiscordContext.Links.CountAsync(link => link.ServerID == ctx.Guild.Id && link.Enabled);
                if (tryFindCountsPerGuild > Configuration.MaxServerLinksPerServer)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent(
                            $"This server is past the discord hook limit of {Configuration.MaxServerLinksPerServer} per server/guild."));
                    return;
                }

                try
                {
                    var msgoutput = await channel.SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Title = "Test Server Alert",
                        Description = "You will get Server Alerts in this format from now on.",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text =
                                $"To disable these notifications, anyone in this discord with Manage Server Permissions can do /unlinkchannel {server}"
                        },
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = "Uncore"
                        }
                    });
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().WithContent(
                            $"Looks like it worked! Check out the channel {channel.Mention}!"));
                }
                catch (UnauthorizedException unauthorized)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent(
                            "The bot needs Send Messages Permission in the channel in order to be able to send out alerts."));
                    return;
                }
                catch (NotFoundException notfound)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent(
                            "The bot needs View Channels Permission in the channel in order to be able to send out alerts. (or the channel you are mentioning, just doesn't exist!"));
                    return;
                }

                var newGameServerLink = new DiscordChannelLink
                {
                    ChannelID = channel.Id,
                    Created = DateTime.UtcNow,
                    Enabled = true,
                    GameServerID = tryGetServer.ServerID,
                    ID = Guid.NewGuid(),
                    LastChanged = DateTime.UtcNow,
                    LastStatus = "Created",
                    ServerID = channel.Guild.Id,
                    UserID = ctx.User.Id
                };
                await DiscordContext.Links.AddAsync(newGameServerLink);
                await DiscordContext.SaveChangesAsync();
            });
        }


        [Command("unlinkchannel")]
        [RequireGuild]
        [Cooldown(2, 5, CooldownBucketType.User)]
        [Description("Unlink a channel from server alerts.")]
        [SlashCommand("unlinkchannel", "This command is used to unlink a channel from server alerts.")]
        public async Task UnLinkChannelCommand(InteractionContext ctx,
            [Option("Server", "Uncore Server ID to get Alerts for")] string server)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            _ = Task.Run(async () =>
            {
                var permissions = ctx.Member.PermissionsIn(ctx.Channel);
                if (permissions.HasPermission(Permissions.ManageMessages) == false && !ctx.Member.IsAdministrator() &&
                    !ctx.Member.IsOwner)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent(
                            "You need Manage Message Permissions in the specified Discord Channel in order to hook up Server Alerts"));
                    return;
                }

                if (Guid.TryParse(server, out var serverGUID) == false)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent(
                            $"Could not parse {server} as a Uncore Server GUID."));
                    return;
                }


                var tryGetLinkServer = await DiscordContext.Links.FirstOrDefaultAsync(link =>
                    link.ServerID == ctx.Guild.Id && link.GameServerID == serverGUID && link.Enabled);
                if (tryGetLinkServer == null)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent(
                            $"This server is not hooked up for notifications in any channel for the server {server}"));
                    return;
                }

                tryGetLinkServer.Enabled = false;
                tryGetLinkServer.LastStatus = $"Disabled by {ctx.User.Username} / {ctx.User.Id}";
                await DiscordContext.SaveChangesAsync();
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"Disabled link from Server {serverGUID} to <#{tryGetLinkServer.ChannelID}>"));
            });
        }
    }
}