using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Blight/PickupValues", fileName = "SO_PickupValues")]
public class PickupValuesSO : ScriptableObject
{
    public float CollectRadius = 1f;
    public float PickupVelocity = 2f;
    public float PickupRotation = 450f;
    public float DisappearWarningTime = 2f;
    public float BlinkSpeed = 4f;
    public float BlinkScale = 0.2f;
    public float SinkSpeed = 0.05f;
    public float SpawnRadius = 1f;
    public float SpawnHeight = 0.5f;
    public float SpawnSpeed = 7.5f;
}
