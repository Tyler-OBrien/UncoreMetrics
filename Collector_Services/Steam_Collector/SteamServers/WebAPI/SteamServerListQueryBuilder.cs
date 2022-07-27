using System.Text;

namespace Steam_Collector.SteamServers.WebAPI;

/// <summary>
///     This class can be used to built up a filter for GetServerList:
///     <see cref="SteamAPI.GetServerList(SteamServerListQueryBuilder, int)" />
///     <para>
///         Spec:
///         <see href="https://developer.valvesoftware.com/wiki/Master_Server_Query_Protocol">
///             Master Server Query Protocol
///             (via Web API)
///         </see>
///     </para>
///     <code>
/// SteamServerListQueryBuilder builder = new SteamServerListQueryBuilder();
/// builder
/// .AppID("730")
/// .Dedicated() // Dedicated Server
/// .Linux() // Linux-based
/// .NotEmpty() // Not Empty
/// .NotFull() // Not Full
/// .Map("cs_italy") // On CS Italy
/// .NoPassword() // No Password
/// .Nor() // None of the following conditions
/// .SpectatorProxies() // Not a spectator proxy
/// .Gametype("valve") // Not a valve Server
/// 
/// SteamAPI steamApi = new SteamAPI("Your-Key");
/// List&lt;SteamListServer&gt; getServers = await steamApi.GetServerList(filter: builder, limit: 1000);
/// </code>
/// </summary>
public class SteamServerListQueryBuilder
{
    private readonly StringBuilder _stringBuilder;

    public SteamServerListQueryBuilder()
    {
        _stringBuilder = new StringBuilder();
    }


    public static SteamServerListQueryBuilder New()
    {
        return new SteamServerListQueryBuilder();
    }

    public override string ToString()
    {
        return _stringBuilder.ToString();
    }


    /// <summary>
    ///     Servers that are running game <paramref name="AppID" />
    /// </summary>
    public SteamServerListQueryBuilder AppID(string AppID)
    {
        _stringBuilder.Append($"\\appid\\{Uri.EscapeDataString(AppID)}");
        return this;
    }

    /// <summary>
    ///     Servers on the specified <paramref name="IP" /> address (port supported and optional)
    /// </summary>
    public SteamServerListQueryBuilder GameAddr(string IP)
    {
        _stringBuilder.Append($"\\gameaddr\\{Uri.EscapeDataString(IP)}");
        return this;
    }

    /// <summary>
    ///     A special filter, specifies that servers matching any of the following [x] conditions should not be returned
    /// </summary>
    public SteamServerListQueryBuilder Nor()
    {
        _stringBuilder.Append("\\nor\\");
        return this;
    }

    /// <summary>
    ///     A special filter, specifies that servers matching all of the following [x] conditions should not be returned
    /// </summary>
    public SteamServerListQueryBuilder Nand()
    {
        _stringBuilder.Append("\\nand\\");
        return this;
    }

    /// <summary>
    ///     Servers flagged as Dedicated Servers
    /// </summary>
    public SteamServerListQueryBuilder Dedicated()
    {
        _stringBuilder.Append("\\dedicated\\1");
        return this;
    }

    /// <summary>
    ///     Servers using anti-cheat technology (VAC, but potentially others as well)
    /// </summary>
    public SteamServerListQueryBuilder Secure()
    {
        _stringBuilder.Append("\\Secure\\1");
        return this;
    }

    /// <summary>
    ///     Servers running the specified modification (<paramref name="game" />) type) (ex. cstrike)
    /// </summary>
    public SteamServerListQueryBuilder Gamedir(string game)
    {
        _stringBuilder.Append($"\\gamedir\\{Uri.EscapeDataString(game)}");
        return this;
    }

    /// <summary>
    ///     Servers running the specified <paramref name="Map" /> (ex. cs_italy)
    /// </summary>
    public SteamServerListQueryBuilder Map(string Map)
    {
        _stringBuilder.Append($"\\gamedir\\{Uri.EscapeDataString(Map)}");
        return this;
    }

    /// <summary>
    ///     Return only Servers running on a Linux platform
    /// </summary>
    public SteamServerListQueryBuilder Linux()
    {
        _stringBuilder.Append("\\linux\\1");
        return this;
    }

    /// <summary>
    ///     Servers that are not password protected
    /// </summary>
    public SteamServerListQueryBuilder NoPassword()
    {
        _stringBuilder.Append("\\password\\0");
        return this;
    }

    /// <summary>
    ///     Servers that are not empty
    /// </summary>
    public SteamServerListQueryBuilder NotEmpty()
    {
        _stringBuilder.Append("\\empty\\1");
        return this;
    }

    /// <summary>
    ///     Servers that are not full
    /// </summary>
    public SteamServerListQueryBuilder NotFull()
    {
        _stringBuilder.Append("\\full\\1");
        return this;
    }


    /// <summary>
    ///     Servers that are spectator proxies
    /// </summary>
    public SteamServerListQueryBuilder SpectatorProxies()
    {
        _stringBuilder.Append("\\proxy\\1");
        return this;
    }

    /// <summary>
    ///     Servers that are NOT running game <paramref name="AppID" /> (This was introduced to block Left 4 Dead games from
    ///     the Steam Server Browser)
    /// </summary>
    public SteamServerListQueryBuilder NotAppID(string AppID)
    {
        _stringBuilder.Append($"\\napp\\{Uri.EscapeDataString(AppID)}");
        return this;
    }

    /// <summary>
    ///     Servers that are empty
    /// </summary>
    public SteamServerListQueryBuilder NoPlayers()
    {
        _stringBuilder.Append("\\noplayers\\1");
        return this;
    }

    /// <summary>
    ///     Servers that are whitelisted
    /// </summary>
    public SteamServerListQueryBuilder Whitelisted()
    {
        _stringBuilder.Append("\\white\\1");
        return this;
    }


    /// <summary>
    ///     Servers with the given <paramref name="tag" /> in sv_tags
    /// </summary>
    public SteamServerListQueryBuilder Gametype(string tag)
    {
        _stringBuilder.Append($"\\gametype\\{Uri.EscapeDataString(tag)}");
        return this;
    }


    /// <summary>
    ///     Servers with all of the given <paramref name="tags" />(s) in sv_tags
    /// </summary>
    public SteamServerListQueryBuilder Gametype(List<string> tags)
    {
        _stringBuilder.Append($"\\gametype\\{string.Join(",", tags.Select(Uri.EscapeDataString))}");
        return this;
    }


    /// <summary>
    ///     Servers with the given <paramref name="tag" /> in their 'hidden' tags (L4D2)
    /// </summary>
    public SteamServerListQueryBuilder Gamedata(string tag)
    {
        _stringBuilder.Append($"\\gamedata\\{Uri.EscapeDataString(tag)}");
        return this;
    }


    /// <summary>
    ///     Servers with all of the given <paramref name="tags" />(s) in their 'hidden' tags (L4D2)
    /// </summary>
    public SteamServerListQueryBuilder Gamedata(List<string> tags)
    {
        _stringBuilder.Append($"\\gamedata\\{string.Join(",", tags.Select(Uri.EscapeDataString))}");
        return this;
    }


    /// <summary>
    ///     Servers with any of the given tag(s) in their 'hidden' tags (L4D2)
    /// </summary>
    public SteamServerListQueryBuilder GamedataOr(string tag)
    {
        _stringBuilder.Append($"\\gamedataor\\{Uri.EscapeDataString(tag)}");
        return this;
    }


    /// <summary>
    ///     Servers with any of the given tag(s) in their 'hidden' tags (L4D2)
    /// </summary>
    public SteamServerListQueryBuilder GamedataOr(List<string> tag)
    {
        _stringBuilder.Append($"\\gamedataor\\{string.Join(",", tag.Select(Uri.EscapeDataString))}");
        return this;
    }


    /// <summary>
    ///     Servers with their hostname matching <paramref name="hostname" /> (can use * as a wildcard)
    /// </summary>
    public SteamServerListQueryBuilder NameMatch(string hostname)
    {
        _stringBuilder.Append($"\\name_match\\{Uri.EscapeDataString(hostname)}");
        return this;
    }

    /// <summary>
    ///     Servers running version <paramref name="version" /> (can use * as a wildcard)
    /// </summary>
    public SteamServerListQueryBuilder Version(string version)
    {
        _stringBuilder.Append($"\\version_match\\{Uri.EscapeDataString(version)}");
        return this;
    }

    /// <summary>
    ///     Only one Server for each unique IP address matched
    /// </summary>
    public SteamServerListQueryBuilder CollapseAddrHash()
    {
        _stringBuilder.Append("\\collapse_addr_hash\\1");
        return this;
    }
}