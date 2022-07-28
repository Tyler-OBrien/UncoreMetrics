namespace UncoreMetrics.Data.GameData;

public enum ValueType
{
    Normal,
    Running
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class GameDataRulesProperty : Attribute
{
    public GameDataRulesProperty(string propertyName)
    {
        PropertyName = propertyName;
        ValueType = ValueType.Normal;
    }

    public GameDataRulesProperty(string template, ValueType valueType, int startIndex = 0)
    {
        PropertyName = template;
        ValueType = valueType;
        StartIndex = startIndex;
    }

    public string PropertyName { get; set; }

    public ValueType ValueType { get; set; }

    public int StartIndex { get; set; }
}