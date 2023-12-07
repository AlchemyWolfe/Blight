using System.Collections.Generic;
using UnityEngine;

public class BlightGameManager : MonoBehaviour
{
    public GameObject EnemyContainer;
    public List<EnemyDefinitionSO> EnemyPools;
    public List<ProjectilePoolSO> ProjectilePools;
    public List<WaveSO> CommonWaves;
    public List<WaveSO> BossWaves;
    public HealthBarPoolSO HealthBarPool;
    public Canvas WorldCanvas;
    public Terrain Ter;
    public GameObject Player;

    void Awake()
    {
        foreach (var pool in EnemyPools)
        {
            pool.Initialize(EnemyContainer);
        }
        foreach (var pool in ProjectilePools)
        {
            pool.Initialize();
        }
        HealthBarPool.Initialize(WorldCanvas);

        CommonWaves[0].StartWave(1, Ter, EnemyContainer, Player);
    }
}
