namespace UncoreMetrics.Data.GameData;

public enum ValueType
{
    Normal,
    Running
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class ServerRulesProperty : Attribute
{
    /// <summary>
    ///     Handles resolving this property from A2S_Rules
    /// </summary>
    /// <param name="propertyName"></param>
    public ServerRulesProperty(string propertyName)
    {
        PropertyName = propertyName;
        ValueType = ValueType.Normal;
    }

    /// <summary>
    ///     Handles resolving this property via A2S_Rules. This type handles "Running" values via the template (processed via
    ///     String.Format, {0} is current count, {1} is amount of found values). Running values are those which use more then
    ///     one key-value pair to bypass A2S_Rules char limit
    /// </summary>
    /// <param name="template">
    ///     Processed via String.Format. {0} is current count, {1} is the count of found a2s_rule elements
    ///     starting with the string before {0}. For example, Project Zomboid uses description:0/6..description:6/6 for
    ///     description tags. This can be handled via description:{0}/{1}. Rust's Format is description_00..description_01,
    ///     which can be handled via description_{0:00}.
    /// </param>
    /// <param name="valueType"></param>
    /// <param name="startIndex">What the count will start at. By default, 0.</param>
    public ServerRulesProperty(string template, ValueType valueType, int startIndex = 0)
    {
        PropertyName = template;
        ValueType = valueType;
        StartIndex = startIndex;
    }

    public string PropertyName { get; set; }

    public ValueType ValueType { get; set; }

    public int StartIndex { get; set; }
}