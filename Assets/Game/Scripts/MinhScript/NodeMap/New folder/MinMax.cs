using UnityEngine;

public class MinMaxSliderAttribute : PropertyAttribute
{
    public float MinLimit { get; }
    public float MaxLimit { get; }

    public MinMaxSliderAttribute(float minLimit, float maxLimit)
    {
        MinLimit = minLimit;
        MaxLimit = maxLimit;
    }
}

[System.Serializable]
public class MinMaxInt
{
    public int min;
    public int max;

    //public MinMaxInt(int min, int max)
    //{
    //    this.min = min;
    //    this.max = max;
    //}

    public int GetValue()
    {
        return Random.Range(min, max + 1);
    }
}

[System.Serializable]
public class MinMaxFloat
{
    public float min;
    public float max;

    //public MinMaxFloat(int min, int max)
    //{
    //    this.min = min;
    //    this.max = max;
    //}

    public float GetValue()
    {
        return Random.Range(min, max);
    }
}