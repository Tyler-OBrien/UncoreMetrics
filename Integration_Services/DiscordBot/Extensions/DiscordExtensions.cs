using System.Runtime.CompilerServices;
using DSharpPlus;
using DSharpPlus.Entities;

namespace DiscordBot.Extensions
{
    public static class DiscordExtensions
    {
        public static bool IsAdministrator(this DiscordMember member)
        {
            return member.Roles.Any(role => role.Permissions.HasPermission(Permissions.Administrator));
        }

        public static bool HasPermission(this DiscordRole role, Permissions permission)
        {
            return role.Permissions.HasPermission(permission);
        }

        public static bool HasPermissionNew(this DiscordMember member, Permissions perm)
        {
            if (member.IsAdministrator() || member.IsOwner)
            {
                return true;
            }

            return !member.Roles.Any()
                ? member.Guild.EveryoneRole.HasPermission(perm)
                : member.Roles.Any(role => role.HasPermission(perm));
        }

        public static DiscordMember? GetMember(this DiscordShardedClient client, ulong id)
        {
            return client
                .ShardClients
                .Values
                .SelectMany(c => c.Guilds.Values)
                .SelectMany(g => g.Members.Values)
                .FirstOrDefault(i => i.Id == id);
        }

        public static DiscordGuild? GetDiscord(this DiscordShardedClient client, ulong id)
        {
            return client
                .ShardClients
                .Values
                .SelectMany(c => c.Guilds.Values)
                .FirstOrDefault(i => i.Id == id);
        }

        public static DiscordMember CreateDiscordMember(this DiscordUser user)
        {
            return Unsafe.As<DiscordUser, DiscordMember>(ref user);
        }
    }
}