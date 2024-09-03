using System;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Blight/EnemyDefinition", fileName = "SO_EnemyDefinition_")]
public class EnemyDefinitionSO : ScriptableObject
{
    [SerializeField]
    public Enemy EnemyPrefab;

    [SerializeField]
    public EnemyDefinitionSO MagicEnemyDefinition;

    [SerializeField]
    public float HP = 1f;
    [SerializeField]
    public float Scale = 1f;
    [SerializeField]
    public float PercievedDifficulty = 1f;
    [SerializeField]
    public bool BalanceFinished = false;

    [SerializeField]
    public float ScoreValue = 10f;
    [SerializeField]
    public float GemDropChance = 0.25f;
    [SerializeField]
    public float GemDropCount = 5;
    [SerializeField]
    public float ShieldDropChance = 0.5f;
    [SerializeField]
    public float ShieldDropCount = 1;

    [SerializeField]
    public WorldHealthBarDefinitionSO HealthBarPool;

    //[SerializeField]
    //public List<Material> Materials;

    [SerializeField, HideInInspector]
    public ObjectPool<Enemy> EnemyPool;

    [SerializeField, HideInInspector]
    public GameObject EnemyContainer;

    [HideInInspector]
    public Action<Enemy> OnEnemyKilledByPlayer;
    public Action<Enemy> OnEnemySpawned;

    public int GetRandomSkinChoice()
    {
        if (EnemyPrefab == null || EnemyPrefab.SkinColors == null || EnemyPrefab.SkinColors.SkinMaterials.Count <= 0)
        {
            return -1;
        }
        return Random.Range(0, EnemyPrefab.SkinColors.SkinMaterials.Count);
    }

    public void Initialize(GameObject enemyContainer)
    {
        EnemyContainer = enemyContainer;
        if (EnemyPool == null)
        {
            EnemyPool = new ObjectPool<Enemy>(OnCreateEnemy, OnGetEnemy, OnReleaseEnemy, OnDestroyEnemy, false, 10, 100);
        }
        EnemyPool.Clear();
        if (MagicEnemyDefinition != null)
        {
            MagicEnemyDefinition.Initialize(enemyContainer);
        }
        if (!BalanceFinished)
        {
            // Standardize rewards, based on HP and PercievedDifficulty.  TODO: Remove this once waves are balanced.
            var rewards = HP * PercievedDifficulty;
            ScoreValue = 10 * rewards * PercievedDifficulty;    // Squaring percieved difficulty.
            GemDropCount = rewards;
            ShieldDropCount = rewards * 0.5f;
        }
    }

    private void OnEnemyKilledByPlayerReceived(Enemy enemy)
    {
        OnEnemyKilledByPlayer?.Invoke(enemy);
    }

    public Enemy CreateEnemy(bool isBoss, GameObject container, GameObject projectileContainer, int skinColor = -1, bool isMagic = false, int extraType = -1)
    {
        if (isMagic && MagicEnemyDefinition != null)
        {
            return MagicEnemyDefinition.CreateEnemy(isBoss, container, projectileContainer, skinColor, isMagic, extraType);
        }

        var enemy = EnemyPool.Get();
        enemy.IsMagic = isMagic;
        enemy.IsBoss = isBoss;
        if (projectileContainer != null)
        {
            enemy.ProjectileContainer = projectileContainer;
        }

        // Set the colors
        if (enemy.SkinColors != null && enemy.SkinColors.SkinMaterials.Count > 0)
        {
            var skinColorCount = enemy.SkinColors.SkinMaterials.Count;
            // Our enemy might have a variety of skins.
            if (skinColor == -1)
            {
                skinColor = Random.Range(0, skinColorCount);
            }
            else
            {
                skinColor = ((skinColor % skinColorCount) + skinColorCount) % skinColorCount;
            }
            enemy.SetSkinColor(skinColor);
        }
        if (isMagic && enemy.MagicColors != null && enemy.MagicColors.MagicMaterials.Count > 0)
        {
            var magicColor = Random.Range(0, enemy.MagicColors.MagicMaterials.Count);
            enemy.SetMagicColor(magicColor);
        }
        else
        {
            enemy.SetMagicColor(-1);
        }
        enemy.gameObject.transform.SetParent(container.transform);

        // Reset stats & state

        OnEnemySpawned?.Invoke(enemy);
        return enemy;
    }

    public void ReturnEnemy(Enemy enemy)
    {
        if (enemy.InUse)
        {
            enemy.StopAttacking();
            enemy.Weapons.Clear();
            enemy.OnKilledByPlayer -= OnEnemyKilledByPlayerReceived;
            EnemyPool.Release(enemy);
        }
    }

    private Enemy OnCreateEnemy()
    {
        var enemy = GameObject.Instantiate(EnemyPrefab);
        enemy.Pool = this;
        return enemy;
    }

    private void OnGetEnemy(Enemy enemy)
    {
        enemy.gameObject.SetActive(true);
        enemy.Reset();
        enemy.OnKilledByPlayer += OnEnemyKilledByPlayerReceived;
        enemy.InUse = true;
    }

    private void OnReleaseEnemy(Enemy enemy)
    {
        enemy.gameObject.SetActive(false);
        enemy.InUse = false;
    }

    private void OnDestroyEnemy(Enemy enemy)
    {
        Destroy(enemy.gameObject);
    }

    public int GetRandomShieldDropCount(float PercievedDifficultyModifier)
    {
        var count = Random.value < ShieldDropChance ? (ShieldDropCount * PercievedDifficultyModifier * (Random.value + 0.5f)) + 0.5f : 0;
        return (int)count;
    }

    public int GetRandomGemDropCount(float PercievedDifficultyModifier)
    {
        var count = Random.value < GemDropChance ? (GemDropCount * PercievedDifficultyModifier * (Random.value + 0.5f)) + 0.5f : 0;
        return (int)count;
    }
}
