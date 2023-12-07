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
    [Tooltip("The first wavecount this type of wave is allowed to spawn.")]
    public int StartingWaveCount = 1;

    [SerializeField]
    public IntPerLevel EnemyCount;

    private void OnValidate()
    {
        EnemyCount.SetMinMax(0, 200);
    }

    public EnemyDefinitionSO GetRandomEnemyDefinition()
    {
        var idx = Random.Range(0, EnemyDefinitions.Count);
        return EnemyDefinitions[idx];
    }

    public Wave StartWave(int waveCount, float duration, Terrain terrain, GameObject container, GameObject player)
    {
        var wave = new Wave(this, terrain, container, player, waveCount, duration);
        wave.SpawnEnemies(duration);
        return wave;
    }
}
