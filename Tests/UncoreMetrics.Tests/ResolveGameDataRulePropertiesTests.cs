using UncoreMetrics.Data.GameData;
using UncoreMetrics.Data.GameData.ARK;
using UncoreMetrics.Data.GameData.ProjectZomboid;
using UncoreMetrics.Data.GameData.Rust;
using UncoreMetrics.Data.GameData.Squad;
using UncoreMetrics.Data.GameData.Unturned;
using UncoreMetrics.Data.GameData.VRising;
using UncoreMetrics.Steam_Collector.Helpers;

namespace UncoreMetrics.Tests;

public class ResolveGameDataRulePropertiesTests
{
    [Fact]
    public void Rust_ResolvesNormalAndRunningProperties()
    {
        var rules = TestRules.Of(
            ("Build", "2541"),
            ("description_00", "Welcome to "),
            ("description_01", "the server"),
            ("ent_cnt", "123456"),
            ("fps", "60"),
            ("hash", "deadbeef"),
            ("pve", "False"),
            ("world.seed", "-12345"),
            ("world.size", "4500"));
        var server = new RustServer();

        server.ResolveGameDataPropertiesFromRules(rules);

        Assert.Equal(2541UL, server.Build);
        Assert.Equal("Welcome to the server", server.Description);
        Assert.Equal(123456, server.EntityCount);
        Assert.Equal(60, server.FPS);
        Assert.Equal("deadbeef", server.Hash);
        Assert.False(server.PvE);
        Assert.Equal(-12345, server.WorldSeed);
        Assert.Equal(4500, server.WorldSize);
    }

    [Fact]
    public void MissingOrInvalidRules_LeavePropertiesNull()
    {
        var rules = TestRules.Of(("fps", "not a number"));
        var server = new RustServer();

        server.ResolveGameDataPropertiesFromRules(rules);

        Assert.Null(server.FPS);
        Assert.Null(server.Build);
        Assert.Null(server.PvE);
        Assert.Null(server.Description);
    }

    [Fact]
    public void Squad_ResolvesLicenseSignatureAcrossKeys()
    {
        var rules = TestRules.Of(
            ("GameMode_s", "RAAS"),
            ("Password_b", "true"),
            ("PlayerCount_i", "98"),
            ("LicenseSig0_s", "abc"),
            ("LicenseSig1_s", "def"));
        var server = new SquadServer();

        server.ResolveGameDataPropertiesFromRules(rules);

        Assert.Equal("RAAS", server.GameMode);
        Assert.True(server.HasPassword);
        Assert.Equal(98, server.PlayerCount);
        Assert.Equal("abcdef", server.LicenseSig);
    }

    [Fact]
    public void VRising_ResolvesEnumCaseInsensitivelyAndNumericBool()
    {
        var rules = TestRules.Of(
            ("castle-heart-damage-mode", "canbedestroyedbyplayers"),
            ("blood-bound-enabled", "1"),
            ("days-runningv2", "12"));
        var server = new VRisingServer();

        server.ResolveGameDataPropertiesFromRules(rules);

        Assert.Equal(CastleHeartDamageMode.CanBeDestroyedByPlayers, server.HeartDamage);
        Assert.True(server.BloodBoundEquipment);
        Assert.Equal(12, server.DaysRunning);
    }

    [Fact]
    public void VRising_InvalidEnumIsIgnored()
    {
        var rules = TestRules.Of(("castle-heart-damage-mode", "NotARealMode"));
        var server = new VRisingServer();

        server.ResolveGameDataPropertiesFromRules(rules);

        Assert.Null(server.HeartDamage);
    }

    [Fact]
    public void Ark_ResolvesRunningList()
    {
        var rules = TestRules.Of(("MOD0_s", "111"), ("MOD1_s", "222"));
        var server = new ArkServer();

        server.ResolveGameDataPropertiesFromRules(rules);

        Assert.Equal(["111", "222"], server.Mods);
    }

    [Fact]
    public void ProjectZomboid_PrefersMultiPartDescriptionWhenPresent()
    {
        var rules = TestRules.Of(
            ("description", "short"),
            ("description:1/2", "long "),
            ("description:2/2", "description"));
        var server = new ProjectZomboidServer();

        server.ResolveGameDataPropertiesFromRules(rules);

        Assert.Equal("long description", server.Description);
    }

    [Fact]
    public void ProjectZomboid_FallsBackToSingleDescription()
    {
        var rules = TestRules.Of(("description", "short"));
        var server = new ProjectZomboidServer();

        server.ResolveGameDataPropertiesFromRules(rules);

        Assert.Equal("short", server.Description);
    }

    [Fact]
    public void Unturned_ResolvesMultiLineBrowserDescription()
    {
        var rules = TestRules.Of(
            ("Browser_Desc_Full_Line_0", "Line one. "),
            ("Browser_Desc_Full_Line_1", "Line two."));
        var server = new UnturnedServer();

        server.ResolveGameDataPropertiesFromRules(rules);

        Assert.Equal("Line one. Line two.", server.BrowserDescription);
    }

    [Fact]
    public void RepeatedCalls_OnSameType_ResolveTheSame()
    {
        // The property metadata is cached per type after the first call
        var first = new SquadServer();
        first.ResolveGameDataPropertiesFromRules(TestRules.Of(("GameMode_s", "RAAS"), ("LicenseSig0_s", "abc")));
        var second = new SquadServer();
        second.ResolveGameDataPropertiesFromRules(TestRules.Of(("GameMode_s", "AAS"), ("LicenseSig0_s", "xyz")));

        Assert.Equal("RAAS", first.GameMode);
        Assert.Equal("abc", first.LicenseSig);
        Assert.Equal("AAS", second.GameMode);
        Assert.Equal("xyz", second.LicenseSig);
    }

    [Fact]
    public void UnsupportedPropertyType_ThrowsEveryTime()
    {
        var rules = TestRules.Of(("ratio", "1.5"));

        Assert.Throws<InvalidOperationException>(() => new UnsupportedModel().ResolveGameDataPropertiesFromRules(rules));
        Assert.Throws<InvalidOperationException>(() => new UnsupportedModel().ResolveGameDataPropertiesFromRules(rules));
    }

    [Fact]
    public void UnsupportedPropertyType_Throws()
    {
        var rules = TestRules.Of(("ratio", "1.5"));

        Assert.Throws<InvalidOperationException>(() =>
            new UnsupportedModel().ResolveGameDataPropertiesFromRules(rules));
    }

    private class UnsupportedModel
    {
        [ServerRulesProperty("ratio")] public double? Ratio { get; set; }
    }
}
