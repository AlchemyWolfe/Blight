using System.Collections.Generic;
using UnityEngine;

public enum WaveFormation
{
    InwardRing
}

[CreateAssetMenu(menuName = "Blight/WaveDefinition", fileName = "SO_Wave_")]
public class WaveSO : ScriptableObject
{
    [SerializeField]
    public List<EnemyDefinitionSO> EnemyDefinitions;

    [SerializeField]
    [Tooltip("This may need to be adjusted for larger enemies if you see them popping in at the edge of the screen.")]
    public float OffScreenRadius = 4f;

    [SerializeField]
    public WaveFormation SpawnFormation;

    [SerializeField]
    public int StartingWaveCount = 1;

    [SerializeField]
    public int InitialEnemyCount = 10;

    [SerializeField]
    [Tooltip("This can have fractional values.  The result will be rounded down.")]
    public float AdditionalEnemyCountPerWave = 4;

    public EnemyDefinitionSO GetRandomEnemyDefinition()
    {
        var idx = Random.Range(0, EnemyDefinitions.Count);
        return EnemyDefinitions[idx];
    }

    public int GetEnemyCount(int waveCount)
    {
        var wavesPastStart = Mathf.Max(0, waveCount - StartingWaveCount);
        return InitialEnemyCount + (int)(AdditionalEnemyCountPerWave * wavesPastStart);
    }

    public Wave StartWave(int waveCount, Terrain terrain, GameObject container, GameObject player)
    {
        var wave = new Wave();
        wave.WaveCount = waveCount;
        wave.Ter = terrain;
        wave.EnemyContainer = container;
        wave.Player = player;
        wave.SpawnEnemies();
        return wave;
    }
}
