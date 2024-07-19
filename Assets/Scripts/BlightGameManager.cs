using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BlightGameManager : MonoBehaviour
{
    public GameOptionsSO Options;
    public GameSceneToolsSO Tools;
    public MenuManager Menus;
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
    public List<WaveSO> RareWaves;
    public float RareWavePercentage = 0.005f;
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

    public AudioClip EnemyDieSound;
    public AudioClip BossDieSound;

    private List<EnemyDefinitionSO> EnemyPools;
    private List<WeaponPoolSO> WeaponPools;
    private float NextCommonWave;
    private float NextBossWave;
    private int ShieldRestoreCount;

    public WaveSO LastCommonWave;
    public WaveSO LastBossWave;
    private List<Wave> CurrentWaves;

    public void InitializeFromWaveSO(WaveSO wave)
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
            InitializeFromWaveSO(wave);
        }
        foreach (var wave in RareWaves)
        {
            InitializeFromWaveSO(wave);
        }
        foreach (var wave in BossWaves)
        {
            InitializeFromWaveSO(wave);
        }
        foreach (var weaponSpec in Options.PlayerWeaponSpecs)
        {
            if (weaponSpec.Weapon && !WeaponPools.Contains(weaponSpec.Weapon))
            {
                WeaponPools.Add(weaponSpec.Weapon);
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
        LastCommonWave = null;
        LastBossWave = null;
        ShieldPickupPool.Initialize();
        GemPickupPool.Initialize();
        UpgradePickupPool.Initialize();
        AddListeners();
        HealthBarPool.Initialize(WorldCanvas);
        Tools.Ter = Ter;
        Tools.IsPlayingGame = false;
        Tools.OnGameStart += OnGameStartReceived;
        Tools.OnGameClose += OnGameCloseReceived;
    }

    private void OnGameCloseReceived()
    {
        Tools.IsPlayingGame = false;

        // Clean up all game data.
        Enemy[] enemies = EnemyContainer.GetComponentsInChildren<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            enemy.Flee();
        }

        Projectile[] projectiles = EnemyProjectileContainer.GetComponentsInChildren<Projectile>();
        foreach (Projectile projectile in projectiles)
        {
            projectile.ReturnToPool();
        }

        // Waves aren't Game Objects, so they need their OnDestroy called.
        foreach (var wave in CurrentWaves)
        {
            wave.OnDestroy();
        }
    }

    public void OnGameStartReceived()
    {
        PlayerData.TotalGems -= Options.CurrentWeaponCost;
        foreach (var weaponSpec in Options.PlayerWeaponSpecs)
        {
            weaponSpec.Weapon.CreateWeapon(Tools.Player, weaponSpec.WeaponLevel, weaponSpec.ProjectileLevel);
        }
        Options.ResetPlayerWeaponCost();
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
        SpawnWave(CommonWaves);
        NextCommonWave = Time.time + FirstCommonWave;
        NextBossWave = Time.time + BossWaveInterval;
        Tools.IsPlayingGame = true;
        Tools.Player.OnKilled += OnPlayerKilledReceived;
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
        if (!enemy.IsBoss && enemy.IsMagic)
        {
            UpgradePickupPool.CreatePickup(enemy.transform, 1, Tools, OnUpgradeCollectedReceived, OnUpgradeExpiredReceived);
        }
        else if (PlayerData.ShieldNeed > 0)
        {
            // Attempt to spawn shield energy.
            var count = Random.value < enemyDefinition.ShieldDropChance ? enemyDefinition.ShieldDropCount : 0;
            if (PlayerData.EarnedShield >= enemyDefinition.ShieldDropCount && Random.value < enemyDefinition.ShieldDropChance)
            {
                count += enemyDefinition.ShieldDropCount;
                PlayerData.EarnedShield -= enemyDefinition.ShieldDropCount;
            }
            for (var i = 0; i < count; ++i)
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
            for (var i = pickups.Length - 1; i >= 0; --i)
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

    public void OnUpgradeCollectedReceived()
    {
        Menus.SwitchMenu(FullscreenMenuType.Upgrade);
    }

    public void OnUpgradeExpiredReceived()
    {
        // This should not happen
        Debug.LogWarning("Upgrade Expired?");
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
            var rareCheck = Random.value;
            if (rareCheck < RareWavePercentage)
            {
                SpawnWave(RareWaves);
            }
            else
            {
                SpawnWave(CommonWaves);
            }
            NextCommonWave += CommonWaveInterval;
        }
        if (Time.time > NextBossWave)
        {
            SpawnWave(BossWaves, true);
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

    public void SpawnWave(List<WaveSO> waves, bool isBossWave = false)
    {
        if (waves.Count <= 0)
        {
            // No waves to spawn.
            return;
        }
        var validWaves = 0;
        foreach (var waveDef in waves)
        {
            if (waveDef.StartingWaveIdx <= CommonWaveInterval)
            {
                validWaves++;
            }
        }
        if (validWaves <= 0)
        {
            // No valid wave?  Hopefully we are testing, and only have one wave to use.
            var waveDef = waves[0];
            var wave = waveDef.StartWave(
                PlayerData.GameWave,
                true,
                CommonWaveDuration,
                EnemyContainer,
                EnemyProjectileContainer,
                Options,
                Tools,
                OnAllEnemiesSpawned,
                OnWaveComplete);
            CurrentWaves.Add(wave);
            if (isBossWave)
            {
                LastBossWave = waveDef;
            }
            else
            {
                LastCommonWave = waveDef;
            }
            return;
        }
        var chosenWave = Random.Range(0, validWaves);
        foreach (var waveDef in waves)
        {
            if (waveDef.StartingWaveIdx <= CommonWaveInterval)
            {
                if (chosenWave <= 0)
                {
                    PlayerData.GameWave++;
                    var wave = waveDef.StartWave(
                        PlayerData.GameWave,
                        isBossWave,
                        CommonWaveDuration,
                        EnemyContainer,
                        EnemyProjectileContainer,
                        Options,
                        Tools,
                        OnAllEnemiesSpawned,
                        OnWaveComplete);
                    CurrentWaves.Add(wave);
                    if (isBossWave)
                    {
                        LastBossWave = waveDef;
                    }
                    else
                    {
                        LastCommonWave = waveDef;
                        if (PlayerData.GameWave > PlayerData.HighestWave)
                        {
                            PlayerData.HighestWave = PlayerData.GameWave;
                        }
                        wave.SetLifetime(CommonWaveInterval * 2);
                    }
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
}
