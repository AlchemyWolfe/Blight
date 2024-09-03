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
    public TargetIndicatorSO TargetIndicatorPool;
    public List<WaveSO> CommonWaves;
    public float FirstCommonWave = 1f;
    public float CommonWaveInterval = 8f;
    public float CommonWaveDuration = 7f;
    public List<WaveSO> RareWaves;
    public float RareWavePercentage = 0.005f;
    public List<WaveSO> BossWaves;
    public WaveSO TestNormalWave;
    public WaveSO TestBossWave;
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
        if (wave == null)
        {
            return;
        }
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
        TargetIndicatorPool.Initialize();
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
        /*
        if (TestNormalWave != null)
        {
            var freeUpgrades = (TestNormalWave.StartingWaveIdx / 2) + 1;
            for (var i = 0; i < freeUpgrades; i++)
            {
                var powerUp = UpgradePickupPool.CreatePickup(Tools.Player.transform, 1, Tools, OnUpgradeCollectedReceived, OnUpgradeExpiredReceived);
                var position = Tools.Player.transform.position;
                var offset = Random.insideUnitCircle * 5f;
                position.x += offset.x;
                position.z += offset.y;
                position.y += 0.2f;

                if (Options.ShowPowerupIndicators)
                {
                    TargetIndicatorPool.CreateIndicator(powerUp.gameObject, Tools.Player, TargetIndicator.IndicatorIcon.PowerUp);
                }
            }
        }*/
    }

    public void AddListeners()
    {
        foreach (var pool in EnemyPools)
        {
            pool.OnEnemyKilledByPlayer += OnEnemyKilledByPlayerReceived;
            pool.OnEnemySpawned += OnEnemySpawned;
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
            var powerUp = UpgradePickupPool.CreatePickup(enemy.transform, 1, Tools, OnUpgradeCollectedReceived, OnUpgradeExpiredReceived);
            if (Options.ShowPowerupIndicators)
            {
                TargetIndicatorPool.CreateIndicator(powerUp.gameObject, Tools.Player, TargetIndicator.IndicatorIcon.PowerUp);
            }
        }
        else if (PlayerData.ShieldNeed > 0)
        {
            // Attempt to spawn shield energy.
            var count = enemyDefinition.GetRandomShieldDropCount(enemy.WaveDefinition.PercievedDifficulty);
            // Including some shield we failed to pick up before.
            if (PlayerData.EarnedShield >= enemyDefinition.ShieldDropCount)
            {
                var extraCount = enemyDefinition.GetRandomShieldDropCount(1f);
                count += extraCount;
                PlayerData.EarnedShield -= extraCount;
            }
            for (var i = 0; i < count; ++i)
            {
                var shield = ShieldPickupPool.CreatePickup(enemy.transform, i, Tools, OnShieldEnergyCollectedReceived, OnShieldEnergyExpiredReceived);
                if (Options.ShowShieldIndicators)
                {
                    TargetIndicatorPool.CreateIndicator(shield.gameObject, Tools.Player, TargetIndicator.IndicatorIcon.Shield);
                }
            }
        }
        else
        {
            // Attempt to spawn gems.
            var count = enemyDefinition.GetRandomGemDropCount(enemy.WaveDefinition.PercievedDifficulty);
            // Including some gems we failed to pick up before.
            if (PlayerData.EarnedGems >= enemyDefinition.GemDropCount)
            {
                var extraCount = enemyDefinition.GetRandomGemDropCount(1f);
                count += extraCount;
                PlayerData.EarnedGems -= extraCount;
            }
            for (var i = 0; i < count; ++i)
            {
                var gem = GemPickupPool.CreatePickup(enemy.transform, i, Tools, OnGemCollectedReceived, OnGemExpiredReceived);
                if (Options.ShowGemIndicators)
                {
                    TargetIndicatorPool.CreateIndicator(gem.gameObject, Tools.Player, TargetIndicator.IndicatorIcon.Gems);
                }
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
        WaveSO chosenWave = null;
        if (!isBossWave && TestNormalWave != null)
        {
            chosenWave = TestNormalWave;
        }
        else if (isBossWave && TestBossWave != null)
        {
            chosenWave = TestBossWave;
        }
        else
        {
            var validWaves = 0;
            foreach (var waveDef in waves)
            {
                if (waveDef.StartingWaveIdx <= CommonWaveInterval)
                {
                    validWaves++;
                }
            }
            var chosenWaveIdx = Random.Range(0, validWaves);
            foreach (var waveDef in waves)
            {
                if (waveDef.StartingWaveIdx <= CommonWaveInterval)
                {
                    if (chosenWaveIdx <= 0)
                    {
                        chosenWave = waveDef;
                        break;
                    }
                    chosenWaveIdx--;
                }
            }
        }
        if (chosenWave == null)
        {
            chosenWave = waves[0];
        }
        if (chosenWave == null)
        {
            return;
        }

        var wave = chosenWave.StartWave(
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
            LastBossWave = chosenWave;
        }
        else
        {
            LastCommonWave = chosenWave;
            PlayerData.GameWave++;
            if (PlayerData.GameWave > PlayerData.HighestWave)
            {
                PlayerData.HighestWave = PlayerData.GameWave;
            }
            wave.SetLifetime(CommonWaveInterval * 2);
        }
    }

    public void OnPlayerKilledReceived()
    {
        PlayerData.Save();
        Tools.OnGameOver?.Invoke();
    }

    public void OnEnemySpawned(Enemy enemy)
    {
        if (TargetIndicatorPool != null)
        {
            var showIndicator = false;// TestNormalWave != null;
            var indicatorIcon = TargetIndicator.IndicatorIcon.Enemy;
            if (enemy.IsBoss)
            {
                showIndicator = true;
                indicatorIcon = TargetIndicator.IndicatorIcon.Boss;
            }
            else if (enemy.IsMagic)
            {
                if (Options.ShowPowerupIndicators)
                {
                    showIndicator = true;
                }
                indicatorIcon = TargetIndicator.IndicatorIcon.Enemy;// PowerUp;
            }
            if (showIndicator)
            {
                TargetIndicatorPool.CreateIndicator(enemy.gameObject, Tools.Player, indicatorIcon);
            }
        }
    }

    public void OnAllEnemiesSpawned(Wave wave)
    {

    }

    public void OnWaveComplete(Wave wave)
    {
        CurrentWaves.Remove(wave);
    }
}
