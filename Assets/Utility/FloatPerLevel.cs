using Sirenix.OdinInspector;
using System;
using UnityEngine;

[Serializable]
public struct FloatPerLevel
{
    public enum PerLevelOperation
    {
        None,
        Add,
        Subtract,
        Multiply
    }

    [SerializeField, HorizontalGroup("PerLevel")]
    public float Initial;

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
    public float Value;

    public void SetMinMax(float min = 0, float max = float.MaxValue)
    {
        Min = min;
        Max = max;
        if (Max < Min)
        {
            Debug.Log("FloatPerLevel: Max less than Min, setting to MaxValue");
            Max = float.MaxValue;
        }
        Initial = Mathf.Clamp(Initial, Min, Max);
    }

    public void ScaleValues(float scale)
    {
        Initial *= scale;
        PerLevel *= scale;
    }

    public void SetLevel(int level)
    {
        Value = Initial;
        Level = level;
        switch (Operation)
        {
            case PerLevelOperation.Add:
                Value += level * PerLevel;
                break;
            case PerLevelOperation.Subtract:
                Value -= level * PerLevel;
                break;
            case PerLevelOperation.Multiply:
                Value += Mathf.Pow(PerLevel, level);
                break;
        }
        Value = Mathf.Clamp(Value, Min, Max);
    }

    public void IncreaseLevel()
    {
        Level++;
        switch (Operation)
        {
            case PerLevelOperation.Add:
                Value += PerLevel;
                break;
            case PerLevelOperation.Subtract:
                Value -= PerLevel;
                break;
            case PerLevelOperation.Multiply:
                Value *= PerLevel;
                break;
        }
        Value = Mathf.Clamp(Value, Min, Max);
    }

    public void DecreaseLevel()
    {
        Level--;
        switch (Operation)
        {
            case PerLevelOperation.Add:
                Value -= PerLevel;
                break;
            case PerLevelOperation.Subtract:
                Value += PerLevel;
                break;
            case PerLevelOperation.Multiply:
                Value /= PerLevel;
                break;
        }
        Value = Mathf.Clamp(Value, Min, Max);
    }
}