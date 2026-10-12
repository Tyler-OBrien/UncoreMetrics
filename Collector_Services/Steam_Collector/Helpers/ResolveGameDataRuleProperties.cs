using System.Collections.Concurrent;
using System.Reflection;
using Okolni.Source.Query.Responses;
using UncoreMetrics.Data.GameData;
using ValueType = UncoreMetrics.Data.GameData.ValueType;

namespace UncoreMetrics.Steam_Collector.Helpers;

public static class ResolveGameDataRuleProperties
{
    /// <summary>A property with one of its <see cref="ServerRulesProperty" /> attributes.</summary>
    private sealed record RuleBinding(PropertyInfo Property, ServerRulesProperty Rule, Type ValueType);

    // This runs for every polled server, so the reflection (GetProperties + GetCustomAttributes, which is slow and
    // allocates) is done once per type. Order matches the uncached version: properties in declaration order, then
    // each property's attributes in order (Project Zomboid's Description depends on this).
    private static readonly ConcurrentDictionary<Type, RuleBinding[]> Bindings = new();

    private static Type TryGetUnderlyingNullable(this Type type)
    {
        var nullableType = Nullable.GetUnderlyingType(type);
        if (nullableType == null)
            return type;
        return nullableType;
    }

    private static RuleBinding[] GetBindings(Type type)
    {
        return Bindings.GetOrAdd(type, static t => t.GetProperties()
            .SelectMany(prop => prop.GetCustomAttributes(true).OfType<ServerRulesProperty>()
                .Select(rule => new RuleBinding(prop, rule, prop.PropertyType.TryGetUnderlyingNullable())))
            .ToArray());
    }

    public static void ResolveGameDataPropertiesFromRules<T>(this T inputClass, RuleResponse rules) where T : class
    {
        foreach (var (prop, rulesProperty, resolvedPropertyType) in GetBindings(inputClass.GetType()))
        {
            var propertyName = rulesProperty.PropertyName;
            if (rulesProperty.ValueType == ValueType.Normal)
            {
                if (resolvedPropertyType == typeof(string))
                {
                    if (rules.TryGetString(propertyName, out var foundValue))
                        prop.SetValue(inputClass, foundValue, null);
                }
                else if (resolvedPropertyType == typeof(bool))
                {
                    if (rules.TryGetBooleanExtended(propertyName, out var foundValue))
                        prop.SetValue(inputClass, foundValue, null);
                }
                else if (resolvedPropertyType == typeof(ulong))
                {
                    if (rules.TryGetUlong(propertyName, out var foundValue))
                        prop.SetValue(inputClass, foundValue, null);
                }
                else if (resolvedPropertyType == typeof(int))
                {
                    if (rules.TryGetInt(propertyName, out var foundValue))
                        prop.SetValue(inputClass, foundValue, null);
                }
                else if (resolvedPropertyType.IsEnum)
                {
                    if (rules.TryGetString(propertyName, out var rawValue) &&
                        Enum.TryParse(resolvedPropertyType, rawValue, true, out var foundValue))
                        prop.SetValue(inputClass, foundValue, null);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Cannot resolve {resolvedPropertyType.Name} / {rulesProperty.PropertyName} for Property {prop.Name} from A2S Rules Dict. This type probably isn't supported.");
                }
            }
            else if (rulesProperty.ValueType == ValueType.Running)
            {
                if (resolvedPropertyType == typeof(string))
                {
                    if (rules.TryGetRunningString(propertyName, out var foundValue, rulesProperty.StartIndex))
                        prop.SetValue(inputClass, foundValue, null);
                }
                else if (resolvedPropertyType == typeof(List<ulong>))
                {
                    prop.SetValue(inputClass, rules.TryGetRunningUlong(propertyName, rulesProperty.StartIndex),
                        null);
                }
                else if (resolvedPropertyType == typeof(List<string>))
                {
                    prop.SetValue(inputClass, rules.TryGetRunningList(propertyName, rulesProperty.StartIndex),
                        null);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Cannot resolve {resolvedPropertyType.Name} / {rulesProperty.PropertyName} for Property {prop.Name} from A2S Rules Dict. This type probably isn't supported.");
                }
            }
        }
    }
}