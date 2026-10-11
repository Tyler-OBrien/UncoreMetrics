using Okolni.Source.Query.Responses;

namespace UncoreMetrics.Tests;

internal static class TestRules
{
    public static RuleResponse Of(params (string Key, string Value)[] rules)
    {
        return new RuleResponse { Rules = rules.ToDictionary(rule => rule.Key, rule => rule.Value) };
    }
}
