using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(menuName = "GameData/GameOptions", fileName = "SO_GameOptions")]
public class GameOptionsSO : ScriptableObject
{
    [SerializeField]
    private bool _mute;
    public bool Mute { get => _mute; set => _mute = value; }

    [SerializeField]
    private float _volume;
    public float Volume { get => _volume; set => _volume = value; }

    [SerializeField]
    private float _gems;
    public float Gems { get => _gems; set => _gems = value; }
}
