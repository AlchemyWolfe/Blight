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
    public float MissedUpgradeRecycleChance = 0.75f;
    public float ExtraUpgradeChance = 0.1f;
    public int ShieldRestoreCost = 3;

    [Tooltip("Each level will apply the PerLevel as a multiplier.")]
    public float BossWaveInterval = 20;
    public WorldHealthBarDefinitionSO HealthBarPool;
    public Camera GameCamera;
    public AudioListener GameAudioListener;
    public Canvas WorldCanvas;
    public Terrain Ter;
    private List<WaveSO> ValidCommonWaves;
    private List<WaveSO> ValidBossWaves;
    private List<WaveSO> ValidRareWaves;

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
        ValidCommonWaves = new List<WaveSO>();
        ValidRareWaves = new List<WaveSO>();
        ValidBossWaves = new List<WaveSO>();
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
                pool.Initialize(EnemyContainer, HealthBarPool);
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
        Physics.IgnoreLayerCollision(19, 19, true); // Ignore collisions on Dead
        Physics.IgnoreLayerCollision(19, 20, true); // Ignore collisions between Player and Dead
        Physics.IgnoreLayerCollision(19, 21, true); // Ignore collisions between Destructible and Dead
        Physics.IgnoreLayerCollision(19, 23, true); // Ignore collisions between Enemy and Dead
    }

    private void OnGameCloseReceived()
    {
        Tools.IsPlayingGame = false;

        // Clean up all game data.
        WorldHealthBar[] healthBars = WorldCanvas.GetComponentsInChildren<WorldHealthBar>();
        foreach (WorldHealthBar healthBar in healthBars)
        {
            healthBar.ReturnToPool();
        }
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
        ValidBossWaves.Clear();
        ValidCommonWaves.Clear();
        ValidRareWaves.Clear();
        AddValidWaves();
        SpawnWave(ValidCommonWaves);
        NextCommonWave = Time.time + FirstCommonWave;
        NextBossWave = Time.time + BossWaveInterval;
        Tools.IsPlayingGame = true;
        Tools.Player.OnKilled += OnPlayerKilledReceived;
        var playerHealthBar = HealthBarPool.CreateHealthBar(Tools.Player.gameObject);
        playerHealthBar.HealthPercent = 1.0f;
        foreach (var weaponSpec in Options.PlayerWeaponSpecs)
        {
            weaponSpec.Weapon.TargetContainer = EnemyContainer;
            weaponSpec.Weapon.SetProjectileMaterial(Tools.Player.MagicMaterial);
        }
    }

    public void AddListeners()
    {
        foreach (var pool in EnemyPools)
        {
            pool.OnEnemyKilledByPlayer += OnEnemyKilledByPlayerReceived;
            pool.OnEnemyEscaped += OnEnemyEscapedReceived;
            pool.OnEnemySpawned += OnEnemySpawned;
            if (pool.MagicEnemyDefinition != null)
            {
                pool.MagicEnemyDefinition.OnEnemyKilledByPlayer += OnEnemyKilledByPlayerReceived;
                pool.MagicEnemyDefinition.OnEnemyEscaped += OnEnemyEscapedReceived;
                pool.MagicEnemyDefinition.OnEnemySpawned += OnEnemySpawned;
            }
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
        if (!Tools.IsPlayingGame)
        {
            return;
        }
        EnemyDefinitionSO enemyDefinition = enemy.Pool;
        var rand = Random.value;
        if (!enemy.IsBoss && enemy.IsMagic)
        {
            var dualUpgradeChance = ExtraUpgradeChance + PlayerData.MissedUpgrades > 0 ? 0.5f : 0f;
            var count = 3 + Random.value < dualUpgradeChance ? 2 : 1;
            for (var i = 0; i < count; ++i)
            {
                var powerUp = UpgradePickupPool.CreatePickup(enemy.transform, 1, Tools, OnUpgradeCollectedReceived, OnUpgradeExpiredReceived);
                if (Options.ShowPowerupIndicators)
                {
                    TargetIndicatorPool.CreateIndicator(powerUp.gameObject, Tools.Player, TargetIndicator.IndicatorIcon.PowerUp);
                }
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

    public void AddValidWaves()
    {
        foreach (var wave in CommonWaves)
        {
            if (wave.StartingWaveIdx == PlayerData.GameWave)
            {
                ValidCommonWaves.Add(wave);
            }
        }
        foreach (var wave in RareWaves)
        {
            if (wave.StartingWaveIdx == PlayerData.GameWave)
            {
                ValidRareWaves.Add(wave);
            }
        }
        foreach (var wave in BossWaves)
        {
            if (wave.StartingWaveIdx == PlayerData.GameWave)
            {
                ValidBossWaves.Add(wave);
            }
        }
    }

    public void OnEnemyEscapedReceived(Enemy enemy)
    {
        if (!enemy.IsBoss && enemy.IsMagic && Random.value < MissedUpgradeRecycleChance)
        {
            PlayerData.MissedUpgrades++;
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
        if (!Tools.IsPlayingGame)
        {
            return;
        }
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
            NextCommonWave += CommonWaveInterval;
            var rareCheck = Random.value;
            if (ValidRareWaves.Count > 0 && rareCheck < RareWavePercentage)
            {
                SpawnWave(ValidRareWaves);
            }
            else
            {
                SpawnWave(ValidCommonWaves);
            }
        }
        if (Time.time > NextBossWave)
        {
            NextBossWave += BossWaveInterval;
            SpawnWave(ValidBossWaves, true);
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
        else if (waves.Count > 0)
        {
            var chosenWaveIdx = Random.Range(0, waves.Count);
            chosenWave = waves[chosenWaveIdx];
        }
        if (chosenWave == null)
        {
            if (isBossWave && BossWaves.Count > 0)
            {
                chosenWave = BossWaves[0];
            }
            else if (!isBossWave && CommonWaves.Count > 0)
            {
                chosenWave = CommonWaves[0];
            }
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
            AddValidWaves();
        }
    }

    public void OnPlayerKilledReceived(BlightCreature player)
    {
        PlayerData.Save();
        Enemy[] enemies = EnemyContainer.GetComponentsInChildren<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            enemy.StopFollowingPlayer();
        }
        Tools.OnGameOver?.Invoke();
    }

    public void OnEnemySpawned(Enemy enemy)
    {
        if (TargetIndicatorPool != null)
        {
            var showIndicator = TestNormalWave != null;
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
                indicatorIcon = TargetIndicator.IndicatorIcon.PowerUp;
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
