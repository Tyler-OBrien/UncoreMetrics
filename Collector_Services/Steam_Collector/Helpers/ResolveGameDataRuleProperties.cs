using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Okolni.Source.Query.Responses;
using UncoreMetrics.Data.GameData;
using ValueType = UncoreMetrics.Data.GameData.ValueType;

namespace Steam_Collector.Helpers
{
    public static class ResolveGameDataRuleProperties
    {

        private static Type TryGetUnderlyingNullable(this Type type)
        {
            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType == null)
                return type;
            return nullableType;
        }

        public static void ResolveGameDataPropertiesFromRules<T>(this T inputClass, RuleResponse rules) where T : class
        {
            PropertyInfo[] props = inputClass.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                var resolvedPropertyType = prop.PropertyType.TryGetUnderlyingNullable();
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    if (attr is GameDataRulesProperty rulesProperty)
                    {
                        string propertyName = rulesProperty.PropertyName;
                        if (rulesProperty.ValueType == ValueType.Normal)
                        {
                            if (resolvedPropertyType == typeof(string))
                            {
                                if (rules.TryGetString(propertyName, out var foundValue))
                                {
                                    prop.SetValue(inputClass, foundValue, null);
                                }
                            }
                            else if (resolvedPropertyType == typeof(bool))
                            {
                                if (rules.TryGetBooleanExtended(propertyName, out var foundValue))
                                {
                                    prop.SetValue(inputClass, foundValue, null);
                                }
                            }
                            else if (resolvedPropertyType == typeof(ulong))
                            {
                                if (rules.TryGetUlong(propertyName, out var foundValue))
                                {
                                    prop.SetValue(inputClass, foundValue, null);
                                }
                            }
                            else if (resolvedPropertyType == typeof(int))
                            {
                                if (rules.TryGetInt(propertyName, out var foundValue))
                                {
                                    prop.SetValue(inputClass, foundValue, null);
                                }
                            }
                            else if (resolvedPropertyType.IsEnum)
                            {
                                if (rules.TryGetString(propertyName, out var rawValue) &&
                                    Enum.TryParse(resolvedPropertyType, rawValue, true, out var foundValue))
                                {
                                    prop.SetValue(inputClass, foundValue, null);
                                }
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
                                {
                                    prop.SetValue(inputClass, foundValue, null);
                                }
                            }
                            else if (resolvedPropertyType == typeof(List<ulong>))
                            {
                                prop.SetValue(inputClass, rules.TryGetRunningUlong(propertyName, rulesProperty.StartIndex), null);
                            }
                            else if (resolvedPropertyType == typeof(List<string>))
                            {
                                prop.SetValue(inputClass, rules.TryGetRunningList(propertyName, rulesProperty.StartIndex), null);
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
        }
    }
}
