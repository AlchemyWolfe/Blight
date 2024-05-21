using System.Collections.Generic;
using UnityEngine;

public enum WaveFormation
{
    HorizontalEdges,
    VerticalEdges,
    HorizontalStream,
    InwardRandom,
    InwardSpiral,
}

public enum WaveMovement
{
    HorizontalStrafe,
    VerticalStrafe,
    AimedStrafe,
    Circling,
    CircleOnce,
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
    public WaveMovement MovementBehavior;

    [SerializeField]
    public WeaponPoolSO Weapon;

    [SerializeField]
    [Tooltip("The first wave index this type of wave is allowed to spawn.")]
    public int StartingWaveIdx = 1;

    public int InitialEnemyCount = 1;
    public int EnemyCountByWave100 = 10;

    // TODO: Add progressive enemy level and/or weapon level?

    public EnemyDefinitionSO GetRandomEnemyDefinition()
    {
        var idx = Random.Range(0, EnemyDefinitions.Count);
        return EnemyDefinitions[idx];
    }

    public Wave StartWave(int waveIdx, float duration, GameObject container, GameObject player, GameOptionsSO options, GameSceneToolsSO tools, Wave.WaveCallback onAllEnemiesSpawned, Wave.WaveCallback onWaveComplete)
    {
        var wave = new Wave(this, container, player, waveIdx, duration, options, tools, onAllEnemiesSpawned, onWaveComplete);
        wave.SpawnEnemies(duration);
        return wave;
    }
}
