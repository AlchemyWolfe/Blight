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

    [SerializeField]
    [Tooltip("GameObject to add Enemy Projectiles as children.  Because this can get cluttery when debugging.")]
    public GameObject EnemyProjectileContainer;

    public List<ProjectilePoolSO> ProjectilePools;
    public List<ExplosionPoolSO> ExplosionPools;
    public List<WaveSO> CommonWaves;
    public float FirstCommonWave = 1f;
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

    public AudioClip EnemyDieSound;
    public AudioClip BossDieSound;

    private List<EnemyDefinitionSO> EnemyPools;
    private List<WeaponPoolSO> WeaponPools;
    private float NextCommonWave;
    private float NextBossWave;
    private int ShieldRestoreCount;

    private List<Wave> CurrentWaves;

    void Awake()
    {
        CurrentWaves = new List<Wave>();
        if (EnemyPools == null)
        {
            EnemyPools = new List<EnemyDefinitionSO>();
        }
        if (WeaponPools == null)
        {
            WeaponPools = new List<WeaponPoolSO>();
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
            if (wave.Weapon && !WeaponPools.Contains(wave.Weapon))
            {
                WeaponPools.Add(wave.Weapon);
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
            if (wave.Weapon && !WeaponPools.Contains(wave.Weapon))
            {
                WeaponPools.Add(wave.Weapon);
            }
        }
        foreach (var pool in EnemyPools)
        {
            if (pool)
            {
                pool.Initialize(EnemyContainer);
            }
        }
        foreach (var pool in WeaponPools)
        {
            if (pool)
            {
                pool.Initialize();
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
        Tools.Ter = Ter;
        Tools.IsPlayingGame = false;
        PlayerWolf.OnKilled += OnPlayerKilledReceived;
        Tools.OnGameStart += OnGameStartReceived;
        Tools.OnGameClose += OnGameCloseReceived;
    }

    private void OnGameCloseReceived()
    {
        Tools.IsPlayingGame = false;
        // TODO: Clean up all game data.
        // Waves aren't Game Objects, so they need their OnDestroy called.
        foreach (var wave in CurrentWaves)
        {
            wave.OnDestroy();
        }
    }

    public void OnGameStartReceived()
    {
        Tools.GameCamera = GameCamera;
        PlayerData.GameWave = 0;
        PlayerData.GameScore = 0;
        PlayerData.GameGems = 0;
        PlayerData.PreviousGems = PlayerData.TotalGems;
        PlayerData.PreviousHighestWave = PlayerData.HighestWave;
        PlayerData.PreviousHighScore = PlayerData.HighScore;
        PlayerData.ShieldNeed = ShieldRestoreCost;
        PlayerData.EarnedGems = 1f;
        PlayerData.EarnedShield = 0f;
        ShieldRestoreCount = 0;
        CurrentWaves.Clear();
        SpawnCommonWave();
        NextCommonWave = Time.time + FirstCommonWave;
        NextBossWave = Time.time + BossWaveInterval;
        Tools.IsPlayingGame = true;
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
        if (PlayerData.GameScore > PlayerData.HighScore)
        {
            PlayerData.HighScore = PlayerData.GameScore;
        }
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
                ShieldPickupPool.CreatePickup(enemy.transform, i, Tools, OnShieldEnergyCollectedReceived, OnShieldEnergyExpiredReceived);
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
                GemPickupPool.CreatePickup(enemy.transform, i, Tools, OnGemCollectedReceived, OnGemExpiredReceived);
            }
        }
    }

    public void OnGemCollectedReceived()
    {
        PlayerData.GameGems++;
        PlayerData.TotalGems++;
    }

    public void OnGemExpiredReceived()
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

    public void OnShieldEnergyExpiredReceived()
    {
        if (Random.value < ShieldPickupRecycleChance)
        {
            PlayerData.EarnedShield++;
        }
    }

    public void Update()
    {
        if (!Tools.IsPlayingGame)
        {
            return;
        }

        Tools.UpdateFrustrum(Tools.Player.transform.position.y);
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
        foreach (var wave in CurrentWaves)
        {
            if (wave.lifetimeEnd > 0 && Time.time > wave.lifetimeEnd)
            {
                wave.Disperse();
            }
        }
    }

    public void SpawnCommonWave()
    {
        var validWaves = 0;
        foreach (var waveDef in CommonWaves)
        {
            if (waveDef.StartingWaveIdx <= CommonWaveInterval)
            {
                validWaves++;
            }
        }
        var chosenWave = Random.Range(0, validWaves);
        foreach (var waveDef in CommonWaves)
        {
            if (waveDef.StartingWaveIdx <= CommonWaveInterval)
            {
                if (chosenWave <= 0)
                {
                    PlayerData.GameWave++;
                    if (PlayerData.GameWave > PlayerData.HighestWave)
                    {
                        PlayerData.HighestWave = PlayerData.GameWave;
                    }
                    var wave = waveDef.StartWave(
                        PlayerData.GameWave,
                        CommonWaveDuration,
                        EnemyContainer,
                        EnemyProjectileContainer,
                        Options,
                        Tools,
                        OnAllEnemiesSpawned,
                        OnWaveComplete);
                    wave.SetLifetime(CommonWaveInterval * 2);
                    CurrentWaves.Add(wave);
                    return;
                }
                chosenWave--;
            }
        }
    }

    public void OnPlayerKilledReceived()
    {
        PlayerData.Save();
        Tools.OnGameOver?.Invoke();
    }

    public void OnAllEnemiesSpawned(Wave wave)
    {

    }

    public void OnWaveComplete(Wave wave)
    {
        CurrentWaves.Remove(wave);
    }

    public void SpawnBossWave()
    {

    }
}
