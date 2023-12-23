using System.Collections.Generic;
using UnityEngine;

public class BlightGameManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("GameObject to add Enemies as children.")]
    public GameObject EnemyContainer;

    public List<ProjectilePoolSO> ProjectilePools;
    public List<WaveSO> CommonWaves;
    public FloatPerLevel CommonWaveInterval;
    public FloatPerLevel CommonWaveDuration;
    public List<WaveSO> BossWaves;

    [Tooltip("Each level will apply the PerLevel as a multiplier.")]
    public FloatPerLevel BossWaveInterval;
    public WorldHealthBarDefinitionSO HealthBarPool;
    public Canvas WorldCanvas;
    public Terrain Ter;
    public Player PlayerWolf;

    private List<EnemyDefinitionSO> EnemyPools;
    private float NextCommonWave;
    private float NextBossWave;

    private Wave CurrentWave;

    private void OnValidate()
    {
        CommonWaveInterval.SetMinMax(1f, 60f);
        BossWaveInterval.SetMinMax(1f, 600f);
    }

    void Awake()
    {
        if (EnemyPools == null)
        {
            EnemyPools = new List<EnemyDefinitionSO>();
        }
        foreach (var wave in CommonWaves)
        {
            foreach (var enemy in wave.EnemyDefinitions)
            {
                if (!EnemyPools.Contains(enemy))
                {
                    EnemyPools.Add(enemy);
                }
            }
        }
        foreach (var wave in BossWaves)
        {
            foreach (var enemy in wave.EnemyDefinitions)
            {
                if (!EnemyPools.Contains(enemy))
                {
                    EnemyPools.Add(enemy);
                }
            }
        }
        foreach (var pool in EnemyPools)
        {
            pool.Initialize(EnemyContainer);
        }
        if (ProjectilePools == null)
        {
            ProjectilePools = new List<ProjectilePoolSO>();
        }
        foreach (var pool in ProjectilePools)
        {
            pool.Initialize();
        }
        HealthBarPool.Initialize(WorldCanvas);
    }

    public void Start()
    {
        CommonWaveInterval.SetLevel(1);
        CommonWaveDuration.SetLevel(1);
        BossWaveInterval.SetLevel(0);
        SpawnCommonWave();
        NextCommonWave = Time.time + CommonWaveInterval.Value;
        NextBossWave = Time.time + BossWaveInterval.Value;
    }

    public void Update()
    {
        if (CurrentWave != null)
        {
            if (CurrentWave.WaveComplete)
            {
                CurrentWave = null;
            }
            return;
        }
        if (Time.time > NextCommonWave)
        {
            SpawnCommonWave();
            CommonWaveInterval.IncreaseLevel();
            CommonWaveDuration.IncreaseLevel();
            NextCommonWave += CommonWaveInterval.Value;
        }
        if (Time.time > NextBossWave)
        {
            SpawnBossWave();
            BossWaveInterval.IncreaseLevel();
            NextBossWave += BossWaveInterval.Value;
        }
    }

    public void SpawnCommonWave()
    {
        var validWaves = 0;
        foreach (var waveDef in CommonWaves)
        {
            if (waveDef.StartingWaveCount <= CommonWaveInterval.Level)
            {
                validWaves++;
            }
        }
        var chosenWave = Random.Range(0, validWaves);
        foreach (var waveDef in CommonWaves)
        {
            if (waveDef.StartingWaveCount <= CommonWaveInterval.Level)
            {
                if (chosenWave <= 0)
                {
                    var wave = waveDef.StartWave(
                        CommonWaveInterval.Level,
                        CommonWaveDuration.Value,
                        Ter,
                        EnemyContainer,
                        PlayerWolf.gameObject);
                    CurrentWave = wave;
                }
                chosenWave--;
            }
        }
    }

    public void SpawnBossWave()
    {

    }
}
