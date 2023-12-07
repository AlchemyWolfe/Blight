using Sirenix.OdinInspector;
using System;
using UnityEngine;

[Serializable]
public struct FloatRange
{
    [SerializeField, HorizontalGroup("MinMax")]
    private float _min;
    public float Min { get => _min; set => _min = value; }

    [SerializeField, HorizontalGroup("MinMax")]
    private float _max;
    public float Max { get => _max; set => _max = value; }

    public FloatRange(float min, float max)
    {
        _min = min;
        _max = max;
    }
}