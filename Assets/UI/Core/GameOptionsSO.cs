using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(menuName = "GameData/GameOptions", fileName = "SO_GameOptions")]
public class GameOptionsSO : ScriptableObject
{
    [SerializeField]
    private bool _mute;
    public bool Mute
    {
        get => _mute;
        set
        {
            _mute = value;
            Save();
        }
    }

    [SerializeField]
    private float _volume;
    public float Volume
    {
        get => _volume;
        set
        {
            _volume = value;
            Save();
        }
    }

    public void Save()
    {
        ES3.Save<GameOptionsSO>("BlightOptions", this, ES3Settings.defaultSettings.path);
    }

    public void Load()
    {
        ES3.LoadInto<GameOptionsSO>("BlightOptions", ES3Settings.defaultSettings.path, this);
    }
}
