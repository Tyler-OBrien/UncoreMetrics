﻿using System.Net;
using Okolni.Source.Common;
using Okolni.Source.Query;
using Okolni.Source.Query.Responses;

namespace Shared_Collectors.Games.Steam.Generic.ServerQuery;

public static class SteamServerQuery
{
    public static (IPAddress address, int port) ParseIPAndPort(string address)
    {
        var lastSemicolonPort = address.LastIndexOf(":", StringComparison.InvariantCultureIgnoreCase);
        int port;
        if (int.TryParse(address.Substring(lastSemicolonPort + 1), out port) == false)
            throw new InvalidOperationException(
                $"Address should be formatted like 1.2.3.4:90, we couldn't find the port and resolve it, we tried to resolve {address.Substring(lastSemicolonPort + 1)}");

        if (IPAddress.TryParse(address.Substring(0, lastSemicolonPort), out var ipAddress) == false)
            throw new InvalidOperationException(
                $"Address should be formatted like 1.2.3.4:90, we couldn't resolve the IP Address from  {address.Substring(0, lastSemicolonPort)}");
        return new ValueTuple<IPAddress, int>(ipAddress, port);
    }


    public static InfoResponse? GetServerInfo(string Host, int Port)
    {
        try
        {
            using var conn = new QueryConnection();


            conn.Host = Host;

            conn.Port = Port;
            conn.ReceiveTimeout = 500;
            conn.SendTimeout = 500;

            conn.Setup();

            var info = conn.GetInfo(5); // Get the Server info

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

    public static async Task<InfoResponse?> GetServerInfoAsync(string Host, int Port)
    {
        try
        {
            using var conn = new QueryConnection();


            conn.Host = Host;

            conn.Port = Port;
            conn.ReceiveTimeout = 500;
            conn.SendTimeout = 500;

            conn.Setup();

            var info = await conn.GetInfoAsync(5); // Get the Server info

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


    public static PlayerResponse? GetPlayers(string Host, int Port)
    {
        try
        {
            using var conn = new QueryConnection();


            conn.Host = Host;
            conn.Port = Port;
            conn.ReceiveTimeout = 500;
            conn.SendTimeout = 500;
            conn.Setup();

            var players =  conn.GetPlayers(5); // Get the Server info

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

    public static async Task<PlayerResponse?> GetPlayersAsync(string Host, int Port)
    {
        try
        {
            using var conn = new QueryConnection();


            conn.Host = Host;
            conn.Port = Port;
            conn.ReceiveTimeout = 500;
            conn.SendTimeout = 500;
            conn.Setup();

            var players = await conn.GetPlayersAsync(5); // Get the Server info

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


    public static RuleResponse? GetRules(string Host, int Port)
    {
        try
        {
            using var conn = new QueryConnection();


            conn.Host = Host;

            conn.Port = Port;
            conn.ReceiveTimeout = 500;
            conn.SendTimeout = 500;

            conn.Setup();

            var rules = conn.GetRules(5); // Get the Server info

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

    public static async Task<RuleResponse?> GetRulesAsync(string Host, int Port)
    {
        try
        {
            using var conn = new QueryConnection();


            conn.Host = Host;

            conn.Port = Port;
            conn.ReceiveTimeout = 500;
            conn.SendTimeout = 500;

            conn.Setup();

            var rules = await conn.GetRulesAsync(5); // Get the Server info

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