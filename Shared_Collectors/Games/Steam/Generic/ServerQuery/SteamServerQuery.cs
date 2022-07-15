using System.Net.Sockets;
using Okolni.Source.Common;
using Okolni.Source.Query;
using Okolni.Source.Query.Responses;


namespace Shared_Collectors.Games.Steam.Generic.ServerQuery;

public static class SteamServerQuery
{

    private static (string address, int port) GetPort(string address)
    {
        var lastSemicolonPort = address.LastIndexOf(":");
        int port;
        if (int.TryParse(address.Substring(lastSemicolonPort + 1), out port) == false)
        {
            throw new InvalidOperationException($"Address should be formatted like 1.2.3.4:90, we couldn't find the port and resolve it, we tried to resolve {address.Substring(lastSemicolonPort + 1)}");
        }
        return new (address.Substring(0, lastSemicolonPort), port);
    }

    public static async Task<InfoResponse?> GetServerInfo(string address)
    {
        try
        {

            using var conn = new QueryConnection();

            var (Host, Port) = GetPort(address);


            conn.Host = Host;

            conn.Port = Port;

            await conn.ConnectAsync(5000);

            var info = await conn.GetInfoAsync(0); // Get the Server info

            return info;
        }
        catch (TimeoutException)
        {
            return null;
        }

        catch (SourceQueryException)
        {
            return null;
        }

        return null;
    }

    public static async Task<PlayerResponse?> GetPlayers(string address)
    {
        try
        {

            using var conn = new QueryConnection();

            var (Host, Port) = GetPort(address);


            conn.Host = Host;

            conn.Port = Port;

            await conn.ConnectAsync(5000);

            var players = await conn.GetPlayersAsync(0); // Get the Server info

            return players;
        }
        catch (TimeoutException)
        {
            return null;
        }

        catch (SourceQueryException)
        {
            return null;
        }

        return null;
    }
    public static async Task<RuleResponse?> GetRules(string address)
    {
        try
        {

            using var conn = new QueryConnection();

            var (Host, Port) = GetPort(address);


            conn.Host = Host;

            conn.Port = Port;

            await conn.ConnectAsync(5000);

            var rules = await conn.GetRulesAsync(0); // Get the Server info

            return rules;
        }
        catch (TimeoutException)
        {
            return null;
        }

        catch (SourceQueryException)
        {
            return null;
        }

        return null;
    }
}