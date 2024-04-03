using System.Collections.Generic;
using UnityEngine;

public class BlightGameManager : MonoBehaviour
{
    public GameOptionsSO Options;
    public GameSceneToolsSO Tools;
    [SerializeField]
    private PlayerDataSO _playerData;
    public PlayerDataSO PlayerData { get => _playerData; set => _playerData = value; }

    [SerializeField]
    [Tooltip("GameObject to add Enemies as children.")]
    public GameObject EnemyContainer;

    public List<ProjectilePoolSO> ProjectilePools;
    public List<ExplosionPoolSO> ExplosionPools;
    public List<WaveSO> CommonWaves;
    public float CommonWaveInterval = 8f;
    public float CommonWaveDuration = 7f;
    public List<WaveSO> BossWaves;
    public PickupPoolSO ShieldPickupPool;
    public float ShieldPickupRecycleChance = 0.5f;
    public PickupPoolSO GemPickupPool;
    public float GemPickupRecycleChance = 0.9f;
    public PickupPoolSO UpgradePickupPool;
    public int ShieldRestoreCost = 3;

    [Tooltip("Each level will apply the PerLevel as a multiplier.")]
    public float BossWaveInterval = 20;
    public WorldHealthBarDefinitionSO HealthBarPool;
    public Camera GameCamera;
    public AudioListener GameAudioListener;
    public Canvas WorldCanvas;
    public Terrain Ter;
    public Player PlayerWolf;
    public BoxCollider InGameBounds;

    public AudioClip EnemyDieSound;
    public AudioClip BossDieSound;

    private List<EnemyDefinitionSO> EnemyPools;
    private float NextCommonWave;
    private float NextBossWave;
    private int ShieldRestoreCount;

    private Wave CurrentWave;

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
                if (enemy && !EnemyPools.Contains(enemy))
                {
                    EnemyPools.Add(enemy);
                }
            }
        }
        foreach (var wave in BossWaves)
        {
            foreach (var enemy in wave.EnemyDefinitions)
            {
                if (enemy && !EnemyPools.Contains(enemy))
                {
                    EnemyPools.Add(enemy);
                }
            }
        }
        foreach (var pool in EnemyPools)
        {
            if (pool)
            {
                pool.Initialize(EnemyContainer);
            }
        }
        if (ProjectilePools == null)
        {
            ProjectilePools = new List<ProjectilePoolSO>();
        }
        foreach (var pool in ProjectilePools)
        {
            if (pool)
            {
                pool.Initialize();
            }
        }
        if (ExplosionPools == null)
        {
            ExplosionPools = new List<ExplosionPoolSO>();
        }
        foreach (var pool in ExplosionPools)
        {
            if (pool)
            {
                pool.Initialize();
            }
        }
        ShieldPickupPool.Initialize();
        GemPickupPool.Initialize();
        UpgradePickupPool.Initialize();
        AddListeners();
        HealthBarPool.Initialize(WorldCanvas);
        Tools.InGameBounds = InGameBounds;
        Tools.Ter = Ter;
    }

    public void Start()
    {
        Tools.AdjustInGameBounds(GameCamera);
        Tools.Player = PlayerWolf;
        PlayerData.GameWave = 0;
        PlayerData.GameScore = 0;
        PlayerData.GameGems = 0;
        PlayerData.ShieldNeed = ShieldRestoreCost;
        PlayerData.EarnedGems = 1f;
        PlayerData.EarnedShield = 0f;
        ShieldRestoreCount = 0;
        PlayerWolf.Shield.DeactivateShield(true);
        SpawnCommonWave();
        NextCommonWave = Time.time + CommonWaveInterval;
        NextBossWave = Time.time + BossWaveInterval;
        PlayerWolf.StartGame();
    }

    public void AddListeners()
    {
        foreach (var pool in EnemyPools)
        {
            pool.OnEnemyKilledByPlayer += OnEnemyKilledByPlayerReceived;
        }
        Tools.OnShieldDown = null;
        Tools.OnShieldDown += OnShieldDownReceived;
    }

    public void OnEnemyKilledByPlayerReceived(Enemy enemy)
    {
        PlayerData.GameScore += enemy.Pool.ScoreValue;
        EnemyDefinitionSO enemyDefinition = enemy.Pool;
        var rand = Random.value;
        if (PlayerData.ShieldNeed > 0)
        {
            // Attempt to spawn shield energy.
            var count = Random.value < enemyDefinition.ShieldDropChance ? enemyDefinition.ShieldDropCount : 0;
            if (PlayerData.EarnedShield >= enemyDefinition.ShieldDropCount && Random.value < enemyDefinition.ShieldDropChance)
            {
                count += enemyDefinition.ShieldDropCount;
                PlayerData.EarnedShield -= enemyDefinition.ShieldDropCount;
            }
            for (var i=0; i<count; ++i)
            {
                ShieldPickupPool.CreatePickup(enemy.transform.position, i, Tools, OnShieldEnergyCollectedReceived, OnShieldEnergyExpiredRecieved);
            }
        }
        else
        {
            // Attempt to spawn gems.
            var count = Random.value < enemyDefinition.GemDropChance ? enemyDefinition.GemDropCount : 0;
            if (PlayerData.EarnedGems >= enemyDefinition.GemDropCount && Random.value < enemyDefinition.GemDropChance)
            {
                count += enemyDefinition.GemDropCount;
                PlayerData.EarnedGems -= enemyDefinition.GemDropCount;
            }
            for (var i = 0; i < count; ++i)
            {
                GemPickupPool.CreatePickup(enemy.transform.position, i, Tools, OnGemCollectedReceived, OnGemExpiredRecieved);
            }
        }
    }

    public void OnGemCollectedReceived()
    {
        PlayerData.GameGems++;
    }

    public void OnGemExpiredRecieved()
    {
        if (Random.value < GemPickupRecycleChance)
        {
            PlayerData.EarnedGems++;
        }
    }

    public void OnShieldDownReceived()
    {
        ShieldRestoreCount++;
        PlayerData.ShieldNeed = ShieldRestoreCost * ShieldRestoreCount;
    }

    public void OnShieldEnergyCollectedReceived()
    {
        if (PlayerData.ShieldNeed <= 0)
        {
            return;
        }
        PlayerData.ShieldNeed -= 1;
        if (PlayerData.ShieldNeed <= 0)
        {
            Tools.Player.Shield.ActivateShield();
            var pickups = FindObjectsOfType<Pickup>();
            for (var i = pickups.Length-1; i >= 0; --i)
            {
                var pickup = pickups[i];
                if (pickup.Type == PickupType.ShieldRestore)
                {
                    pickup.ReturnToPool();
                }
            }
        }
    }

    public void OnShieldEnergyExpiredRecieved()
    {
        if (Random.value < ShieldPickupRecycleChance)
        {
            PlayerData.EarnedShield++;
        }
    }

    public void Update()
    {
        if (CurrentWave != null)
        {
            if (CurrentWave.WaveComplete)
            {
                CurrentWave = null;
            }
        }
        if (Time.time > NextCommonWave)
        {
            SpawnCommonWave();
            NextCommonWave += CommonWaveInterval;
        }
        if (Time.time > NextBossWave)
        {
            SpawnBossWave();
            NextBossWave += BossWaveInterval;
        }
    }

    public void SpawnCommonWave()
    {
        var validWaves = 0;
        foreach (var waveDef in CommonWaves)
        {
            if (waveDef.StartingWaveCount <= CommonWaveInterval)
            {
                validWaves++;
            }
        }
        var chosenWave = Random.Range(0, validWaves);
        foreach (var waveDef in CommonWaves)
        {
            if (waveDef.StartingWaveCount <= CommonWaveInterval)
            {
                if (chosenWave <= 0)
                {
                    PlayerData.GameWave++;
                    var wave = waveDef.StartWave(
                        PlayerData.GameWave,
                        CommonWaveDuration,
                        EnemyContainer,
                        PlayerWolf.gameObject,
                        Options,
                        Tools);
                    CurrentWave = wave;
                    return;
                }
                chosenWave--;
            }
        }
    }

    public void SpawnBossWave()
    {

    }
}
