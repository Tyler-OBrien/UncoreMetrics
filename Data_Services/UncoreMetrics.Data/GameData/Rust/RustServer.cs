namespace UncoreMetrics.Data.GameData.Rust;

public class RustServer : Server
{
    [ServerRulesProperty("Build")] public ulong? Build { get; set; }


    [ServerRulesProperty("description_{0:00}", ValueType.Running)]
    public string? Description { get; set; }

    [ServerRulesProperty("ent_cnt")] public int? EntityCount { get; set; }


    [ServerRulesProperty("fps")] public int? FPS { get; set; }


    [ServerRulesProperty("fps_avg")] public int? AverageFPS { get; set; }


    [ServerRulesProperty("gc_cl")] public int? gc_cl { get; set; }


    [ServerRulesProperty("gc_mb")] public int? gc_mb { get; set; }


    [ServerRulesProperty("hash")] public string? Hash { get; set; }


    [ServerRulesProperty("headerimage")] public string? HeaderImage { get; set; }

    [ServerRulesProperty("logoimage")] public string? LogoImage { get; set; }


    [ServerRulesProperty("pve")] public bool? PvE { get; set; }


    [ServerRulesProperty("uptime")] public int? Uptime { get; set; }


    [ServerRulesProperty("url")] public string? URL { get; set; }


    [ServerRulesProperty("world.seed")] public int? WorldSeed { get; set; }


    [ServerRulesProperty("world.size")] public int? WorldSize { get; set; }
}