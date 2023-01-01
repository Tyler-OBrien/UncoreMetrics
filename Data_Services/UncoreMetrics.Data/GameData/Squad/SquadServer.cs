namespace UncoreMetrics.Data.GameData.Squad
{
    public class SquadServer : Server
    {
        [ServerRulesProperty("Flags_i")] public int? Flags { get; set; }
        [ServerRulesProperty("GameMode_s")] public string? GameMode { get; set; }
        [ServerRulesProperty("GameVersion_s")] public string? GameVersion { get; set; }

        [ServerRulesProperty("NUMOPENPRIVCONN")]
        public int? OPENPRIVCONN { get; set; }

        [ServerRulesProperty("NUMOPENPUBCONN")]
        public int? NUMOPENPUBCONN { get; set; }

        [ServerRulesProperty("NUMPRIVCONN")] public int? NUMPRIVCONN { get; set; }
        [ServerRulesProperty("NUMPUBCONN")] public int? NUMPUBCONN { get; set; }
        [ServerRulesProperty("Password_b")] public bool? HasPassword { get; set; }
        [ServerRulesProperty("PlayerCount_i")] public int? PlayerCount { get; set; }

        [ServerRulesProperty("PlayerReserveCount_i")]
        public int? PlayerReserveCount { get; set; }

        [ServerRulesProperty("PublicQueueLimit_i")]
        public int? PublicQueueLimit { get; set; }

        [ServerRulesProperty("PublicQueue_i")] public int? PublicQueue { get; set; }

        [ServerRulesProperty("ReservedQueue_i")]
        public int? ReservedQueue { get; set; }

        [ServerRulesProperty("SEARCHKEYWORDS_s")]
        public string? SEARCHKEYWORDS { get; set; }

        [ServerRulesProperty("SESSIONFLAGS")] public int? SESSIONFLAGS { get; set; }
        [ServerRulesProperty("TeamOne_s")] public string? TeamOne { get; set; }
        [ServerRulesProperty("TeamTwo_s")] public string? TeamTwo { get; set; }
        [ServerRulesProperty("LicenseId_i")] public int? LicenseID { get; set; }

        [ServerRulesProperty("LicenseSig{0}_s", ValueType.Running)]
        public string? LicenseSig { get; set; }

        public bool? ValidLicense { get; set; }
    }
}