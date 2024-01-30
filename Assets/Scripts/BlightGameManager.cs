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
    public FloatPerLevel CommonWaveInterval;
    public FloatPerLevel CommonWaveDuration;
    public List<WaveSO> BossWaves;
    public PickupPoolSO ShieldPickupPool;
    public PickupPoolSO GemPickupPool;
    public PickupPoolSO UpgradePickupPool;
    public int ShieldRestoreCost = 3;

    [Tooltip("Each level will apply the PerLevel as a multiplier.")]
    public FloatPerLevel BossWaveInterval;
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
        if (ExplosionPools == null)
        {
            ExplosionPools = new List<ExplosionPoolSO>();
        }
        foreach (var pool in ExplosionPools)
        {
            pool.Initialize();
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
        CommonWaveInterval.SetLevel(1);
        CommonWaveDuration.SetLevel(1);
        BossWaveInterval.SetLevel(0);
        SpawnCommonWave();
        NextCommonWave = Time.time + CommonWaveInterval.Value;
        NextBossWave = Time.time + BossWaveInterval.Value;
        PlayerData.GameScore = 0;
        PlayerData.GameGems = 0;
        PlayerData.ShieldNeed = ShieldRestoreCost;
        PlayerData.EarnedGems = 1f;
        PlayerData.EarnedShield = 0f;
        ShieldRestoreCount = 0;
        PlayerWolf.Shield.DeactivateShield(true);
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
            PlayerData.EarnedShield += enemyDefinition.ShieldValue;
            // Attempt to spawn shield energy.
            var count = enemyDefinition.GetDroppedShield(PlayerData.EarnedShield);
            for (var i=0; i<count; ++i)
            {
                ShieldPickupPool.CreatePickup(enemy.transform.position, Tools, OnShieldEnergyCollectedReceived);
            }
        }
        else
        {
            PlayerData.EarnedGems += enemyDefinition.GemValue;
            // Attempt to spawn gems.
            var count = enemyDefinition.GetDroppedGems(PlayerData.EarnedGems);
            for (var i = 0; i < count; ++i)
            {
                GemPickupPool.CreatePickup(enemy.transform.position, Tools, OnGemCollectedReceived);
            }
        }
    }

    public void OnGemCollectedReceived()
    {
        PlayerData.GameGems++;
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
                        EnemyContainer,
                        PlayerWolf.gameObject,
                        Options,
                        Tools);
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
