using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GameData/GameOptions", fileName = "SO_GameOptions")]
public class GameOptionsSO : ScriptableObject
{
    public bool Mute;
    public float Volume;
    public float Gems;
}
