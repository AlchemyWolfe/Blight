using Sirenix.OdinInspector;
using System;
using UnityEngine;

[Serializable]
public struct IntPerLevel
{
    public enum PerLevelOperation
    {
        None,
        Add,
        Subtract,
        Multiply
    }

    [SerializeField, HorizontalGroup("PerLevel")]
    public int Initial;

    [SerializeField, HorizontalGroup("PerLevel")]
    public float PerLevel;

    [SerializeField, HorizontalGroup("Controls")]
    public float Min;

    [SerializeField, HorizontalGroup("Controls")]
    public float Max;

    [SerializeField, HorizontalGroup("Controls")]
    public PerLevelOperation Operation;

    [HideInInspector]
    public int Level;

    [HideInInspector]
    public int Value;

    [HideInInspector]
    public float FractionalValue;

    [HideInInspector]
    public float ScaledInitial;

    public void SetMinMax(int min = 0, int max = int.MaxValue)
    {
        Min = min;
        Max = max;
        if (Max < Min)
        {
            Debug.Log("IntPerLevel: Max less than Min, setting to MaxValue");
            Max = int.MaxValue;
        }
        ScaledInitial = ScaledInitial == 0
            ? Mathf.Clamp((float)Initial, Min, Max)
            : Mathf.Clamp(ScaledInitial, Min, Max);
    }

    public void ScaleValues(float scale)
    {
        ScaledInitial = scale * (float)Initial;
        PerLevel *= scale;
    }

    public void SetLevel(int level)
    {
        FractionalValue = ScaledInitial == 0f ? Initial : ScaledInitial;
        Level = level;
        switch (Operation)
        {
            case PerLevelOperation.Add:
                FractionalValue += level * PerLevel;
                break;
            case PerLevelOperation.Subtract:
                FractionalValue -= level * PerLevel;
                break;
            case PerLevelOperation.Multiply:
                FractionalValue += Mathf.Pow(PerLevel, level);
                break;
        }
        Value = (int)Mathf.Clamp(FractionalValue, Min, Max);
    }

    public void IncreaseLevel()
    {
        Level++;
        switch (Operation)
        {
            case PerLevelOperation.Add:
                FractionalValue += PerLevel;
                break;
            case PerLevelOperation.Subtract:
                FractionalValue -= PerLevel;
                break;
            case PerLevelOperation.Multiply:
                FractionalValue *= PerLevel;
                break;
        }
        Value = (int)Mathf.Clamp(FractionalValue, Min, Max);
    }

    public void DecreaseLevel()
    {
        Level--;
        switch (Operation)
        {
            case PerLevelOperation.Add:
                FractionalValue -= PerLevel;
                break;
            case PerLevelOperation.Subtract:
                FractionalValue += PerLevel;
                break;
            case PerLevelOperation.Multiply:
                FractionalValue /= PerLevel;
                break;
        }
        Value = (int)Mathf.Clamp(FractionalValue, Min, Max);
    }
}