using Okolni.Source.Common;
using UncoreMetrics.Steam_Collector.Helpers;

namespace UncoreMetrics.Tests;

public class A2SRulesHelperTests
{
    [Theory]
    [InlineData("True", true)]
    [InlineData("false", false)]
    public void TryGetBoolean_ParsesBooleanStrings(string raw, bool expected)
    {
        var rules = TestRules.Of(("pve", raw));

        Assert.True(rules.TryGetBoolean("pve", out var value));
        Assert.Equal(expected, value);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("0")]
    [InlineData("yes")]
    public void TryGetBoolean_RejectsNonBooleanStrings(string raw)
    {
        var rules = TestRules.Of(("pve", raw));

        Assert.False(rules.TryGetBoolean("pve", out _));
    }

    [Theory]
    [InlineData("1", true)]
    [InlineData("0", false)]
    [InlineData("TRUE", true)]
    [InlineData("False", false)]
    public void TryGetBooleanExtended_ParsesNumericAndTextBooleans(string raw, bool expected)
    {
        var rules = TestRules.Of(("pve", raw));

        Assert.True(rules.TryGetBooleanExtended("pve", out var value));
        Assert.Equal(expected, value);
    }

    [Theory]
    [InlineData("2")]
    [InlineData("yes")]
    [InlineData("")]
    public void TryGetBooleanExtended_RejectsUnknownValues(string raw)
    {
        var rules = TestRules.Of(("pve", raw));

        Assert.False(rules.TryGetBooleanExtended("pve", out var value));
        Assert.False(value);
    }

    [Fact]
    public void TryGetters_ReturnFalseWhenRuleIsMissing()
    {
        var rules = TestRules.Of();

        Assert.False(rules.TryGetBoolean("missing", out _));
        Assert.False(rules.TryGetBooleanExtended("missing", out _));
        Assert.False(rules.TryGetString("missing", out _));
        Assert.False(rules.TryGetInt("missing", out _));
        Assert.False(rules.TryGetUlong("missing", out _));
    }

    [Fact]
    public void TryGetString_ReturnsRawValue()
    {
        var rules = TestRules.Of(("hash", "abc123"));

        Assert.True(rules.TryGetString("hash", out var value));
        Assert.Equal("abc123", value);
    }

    [Theory]
    [InlineData("42", true, 42)]
    [InlineData("-7", true, -7)]
    [InlineData("4.2", false, 0)]
    [InlineData("abc", false, 0)]
    public void TryGetInt_ParsesIntegers(string raw, bool success, int expected)
    {
        var rules = TestRules.Of(("fps", raw));

        Assert.Equal(success, rules.TryGetInt("fps", out var value));
        Assert.Equal(expected, value);
    }

    [Theory]
    [InlineData("18446744073709551615", true, ulong.MaxValue)]
    [InlineData("-1", false, 0UL)]
    public void TryGetUlong_ParsesUnsignedLongs(string raw, bool success, ulong expected)
    {
        var rules = TestRules.Of(("Build", raw));

        Assert.Equal(success, rules.TryGetUlong("Build", out var value));
        Assert.Equal(expected, value);
    }

    [Fact]
    public void TryGetRunningString_ConcatenatesUntilGap()
    {
        var rules = TestRules.Of(
            ("description_00", "Hello "),
            ("description_01", "World"),
            ("description_03", "skipped because 02 is missing"));

        Assert.True(rules.TryGetRunningString("description_{0:00}", out var value));
        Assert.Equal("Hello World", value);
    }

    [Fact]
    public void TryGetRunningString_RespectsStartIndexAndTotalPlaceholder()
    {
        // Project Zomboid style: description:<index>/<total>, starting at 1
        var rules = TestRules.Of(
            ("description:1/2", "Part one, "),
            ("description:2/2", "part two"));

        Assert.True(rules.TryGetRunningString("description:{0}/{1}", out var value, 1));
        Assert.Equal("Part one, part two", value);
    }

    [Fact]
    public void TryGetRunningString_ReturnsFalseWhenNothingFound()
    {
        var rules = TestRules.Of(("other", "value"));

        Assert.False(rules.TryGetRunningString("desc{0}", out var value));
        Assert.Equal(string.Empty, value);
    }

    [Fact]
    public void TryGetRunningList_ReturnsEachItem()
    {
        var rules = TestRules.Of(("MOD0_s", "first"), ("MOD1_s", "second"));

        Assert.Equal(["first", "second"], rules.TryGetRunningList("MOD{0}_s"));
    }

    [Fact]
    public void TryGetRunningUlong_StopsAtFirstNonNumber()
    {
        var rules = TestRules.Of(("id0", "1"), ("id1", "2"), ("id2", "nope"), ("id3", "4"));

        Assert.Equal([1UL, 2UL], rules.TryGetRunningUlong("id{0}"));
    }

    [Theory]
    [InlineData("pve", true)]
    [InlineData("missing", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void DoesPropertyExist_ChecksKeys(string name, bool expected)
    {
        var rules = TestRules.Of(("pve", "1"));

        Assert.Equal(expected, rules.DoesPropertyExist(name));
    }

    [Theory]
    [InlineData(Enums.Environment.Linux, 'l')]
    [InlineData(Enums.Environment.Mac, 'm')]
    [InlineData(Enums.Environment.Windows, 'w')]
    public void ResolveEnvironment_MapsToSteamChar(Enums.Environment environment, char expected)
    {
        Assert.Equal(expected, environment.ResolveEnvironment());
    }
}
