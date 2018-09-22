public class MinMaxFloat
{
    public float Min { get; private set; }
    public float Max { get; private set; }

    public MinMaxFloat()
    {
        Min = float.MaxValue;
        Max = float.MinValue;
    }

    public void AddValue(float value)
    {
        if (value < Min)
        {
            Min = value;
        }
        if (value > Max)
        {
            Max = value;
        }
    }
}
