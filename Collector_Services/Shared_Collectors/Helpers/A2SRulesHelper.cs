using System.Text;
using Okolni.Source.Query.Responses;
using SQLitePCL;

namespace Shared_Collectors.Helpers;

public static class A2SRulesHelper
{
    /// <summary>
    /// Try get Boolean from Rule Response Dictionary with specified name.
    /// </summary>
    /// <param name="ruleResponse"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns>Bool, if succeeded or not</returns>
    public static bool TryGetBoolean(this RuleResponse ruleResponse, string name, out bool value)
    {
        value = false;
        if (ruleResponse.Rules.TryGetValue(name, out var rawvalue) == false)
            return false;
        if (bool.TryParse(rawvalue, out value))
            return true;
        return false;
    }
    /// <summary>
    /// Try get Boolean from Rule Response Dictionary with specified name. Extended, will try to resolve other strings as bools as well. For example "1" will return as true.
    /// </summary>
    /// <param name="ruleResponse"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns>Bool, if succeeded or not</returns>
    public static bool TryGetBooleanExtended(this RuleResponse ruleResponse, string name, out bool value)
    {
        value = false;
        if (ruleResponse.Rules.TryGetValue(name, out var rawvalue) == false)
            return false;
        if (bool.TryParse(rawvalue, out value))
            return true;
        if (string.Equals(rawvalue, "1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(rawvalue, "true", StringComparison.OrdinalIgnoreCase))
        {
            value = true;
            return true;
        }
        if (!string.Equals(rawvalue, "0", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(rawvalue, "false", StringComparison.OrdinalIgnoreCase)) return false;
        value = false;
        return true;
    }
    /// <summary>
    /// Try get String from Rule Response Dictionary with specified name
    /// </summary>
    /// <param name="ruleResponse"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns>Bool, if succeeded or not</returns>
    public static bool TryGetString(this RuleResponse ruleResponse, string name, out string value)
    {
        value = string.Empty;
        if (ruleResponse.Rules.TryGetValue(name, out value) == false)
            return false;

        return true;
    }
    /// <summary>
    /// Try get Int from Rule Response Dictionary with specified name
    /// </summary>
    /// <param name="ruleResponse"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns>Bool, if succeeded or not</returns>
    public static bool TryGetInt(this RuleResponse ruleResponse, string name, out int value)
    {
        value = 0;
        if (ruleResponse.Rules.TryGetValue(name, out var rawvalue) == false)
            return false;
        if (int.TryParse(rawvalue, out value))
            return true;
        return false;
    }
    /// <summary>
    /// Try get enum from Rule Response Dictionary with specified name
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    /// <param name="ruleResponse"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns>Bool, if succeeded or not</returns>
    public static bool TryGetEnum<TEnum>(this RuleResponse ruleResponse, string name, out TEnum value)
        where TEnum : struct
    {
        value = default;
        if (ruleResponse.Rules.TryGetValue(name, out var rawvalue) == false)
            return false;
        if (Enum.TryParse(rawvalue, out value))
            return true;
        return false;
    }
    /// <summary>
    /// Try get Ulong from Rule Response Dictionary with specified name
    /// </summary>
    /// <param name="ruleResponse"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns>Bool, if succeeded or not</returns>
    public static bool TryGetUlong(this RuleResponse ruleResponse, string name, out ulong value)
    {
        value = 0;
        if (ruleResponse.Rules.TryGetValue(name, out var rawvalue) == false)
            return false;
        if (ulong.TryParse(rawvalue, out value))
            return true;
        return false;
    }
    /// <summary>
    /// Replaces {0} in template to grab item from A2S_Rules Dictionary (i.e desc0) and returns back list of found items
    /// </summary>
    /// <param name="ruleResponse">The Rules Response</param>
    /// <param name="template">Inserts current number at {0}</param>
    /// <param name="startAt">Number to start counting at</param>
    /// <returns>List of found strings based off template</returns>
    public static bool TryGetRunningString(this RuleResponse ruleResponse, string template, out string value,
        int startAt = 0)
    {
        var stringBuilder = new StringBuilder();
        var count = startAt;
        while (true)
        {
            if (ruleResponse.TryGetString(template.Replace("{0}", count.ToString()), out var item))
                stringBuilder.Append(item);
            else
                // Break when no more descriptions are found....
                break;
            count++;
        }

        value = stringBuilder.ToString();
        if (stringBuilder.Length > 0)
            return true;
        return false;
    }

    /// <summary>
    /// Replaces {0} in template to grab item from A2S_Rules Dictionary (i.e desc0) and returns back list of found items
    /// </summary>
    /// <param name="ruleResponse">The Rules Response</param>
    /// <param name="template">Inserts current number at {0}</param>
    /// <param name="startAt">Number to start counting at</param>
    /// <returns>List of found strings based off template</returns>
    public static List<string> TryGetRunningList(this RuleResponse ruleResponse, string template, int startAt = 0)
    {
        var items = new List<string>();
        var count = startAt;
        while (true)
        {
            if (ruleResponse.TryGetString(template.Replace("{0}", count.ToString()), out var rawItem))
                items.Add(rawItem);
            else
                // Break when no more descriptions are found....
                break;
            count++;
        }

        return items;
    }
    /// <summary>
    /// Replaces {0} in template to grab item from A2S_Rules Dictionary (i.e desc0) and returns back list of found items converted to ulong.
    /// </summary>
    /// <param name="ruleResponse">The Rules Response</param>
    /// <param name="template">Inserts current number at {0}</param>
    /// <param name="startAt">Number to start counting at</param>
    /// <returns>List of found ulongs based off template</returns>
    public static List<ulong> TryGetRunningUlong(this RuleResponse ruleResponse, string template, int startAt = 0)
    {
        var items = new List<ulong>();
        var count = startAt;
        while (true)
        {
            if (ruleResponse.TryGetString(template.Replace("{0}", count.ToString()), out var rawItem) && ulong.TryParse(rawItem, out var item))
                items.Add(item);
            else
                // Break when no more descriptions are found....
                break;
            count++;
        }

        return items;
    }
}