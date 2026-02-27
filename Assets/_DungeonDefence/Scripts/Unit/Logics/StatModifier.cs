public class StatModifier
{
    public float Value { get; private set; }
    public StatModType Type { get; private set; }
    public object Source { get; private set; }

    public StatModifier(float value, StatModType type, object source)
    {
        Value = value;
        Type = type;
        Source = source;
    }
}
