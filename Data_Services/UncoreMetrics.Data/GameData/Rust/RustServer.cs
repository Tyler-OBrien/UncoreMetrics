namespace UncoreMetrics.Data.GameData.Rust;

public class RustServer : Server
{
    [GameDataRulesProperty("Build")] public ulong? Build { get; set; }


    [GameDataRulesProperty("description_{0:00}", ValueType.Running)]
    public string? Description { get; set; }

    [GameDataRulesProperty("ent_cnt")] public int? EntityCount { get; set; }


    [GameDataRulesProperty("fps")] public int? FPS { get; set; }


    [GameDataRulesProperty("fps_avg")] public int? AverageFPS { get; set; }


    [GameDataRulesProperty("gc_cl")] public int? gc_cl { get; set; }


    [GameDataRulesProperty("gc_mb")] public int? gc_mb { get; set; }


    [GameDataRulesProperty("hash")] public string? Hash { get; set; }


    [GameDataRulesProperty("headerimage")] public string? HeaderImage { get; set; }

    [GameDataRulesProperty("logoimage")] public string? LogoImage { get; set; }


    [GameDataRulesProperty("pve")] public bool? PvE { get; set; }


    [GameDataRulesProperty("uptime")] public int? Uptime { get; set; }


    [GameDataRulesProperty("url")] public string? URL { get; set; }


    [GameDataRulesProperty("world.seed")] public int? WorldSeed { get; set; }


    [GameDataRulesProperty("world.size")] public int? WorldSize { get; set; }
}