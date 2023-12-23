using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(menuName = "Blight/EnemyDefinition", fileName = "SO_EnemyDefinition_")]
public class EnemyDefinitionSO : ScriptableObject
{
    [SerializeField]
    public Enemy EnemyPrefab;

    [SerializeField]
    public WorldHealthBarDefinitionSO HealthBarPool;

    [SerializeField]
    public List<Material> Materials;

    [SerializeField, HideInInspector]
    public ObjectPool<Enemy> EnemyPool;

    [SerializeField, HideInInspector]
    public GameObject EnemyContainer;

    public void Initialize(GameObject enemyContainer)
    {
        EnemyContainer = enemyContainer;
        if (EnemyPool == null)
        {
            EnemyPool = new ObjectPool<Enemy>(OnCreateEnemy, OnGetEnemy, OnReleaseEnemy, OnDestroyEnemy, false, 10, 100);
        }
        EnemyPool.Clear();
    }

    public Enemy CreateEnemy(GameObject container, int material = -1, bool useSecondarySkin = false, bool isMagic = false, int extraType = -1)
    {
        var enemy = EnemyPool.Get();

        // Set the colors
        if (Materials != null && Materials.Count > 0)
        {
            // Our template might specify a specific skin.
            if (material == -1)
            {
                material = Random.Range(0, Materials.Count);
            }
            else
            {
                material = ((material % Materials.Count) + Materials.Count) % Materials.Count;
            }
            var chosenMaterial = Materials[material];
            enemy.SetSkin(chosenMaterial);
        }
        else if (enemy.SkinMaterials != null && enemy.SkinMaterials != null)
        {
            // Our enemy might have a variety of skins.
            if (material == -1)
            {
                material = Random.Range(0, enemy.SkinMaterials.Count);
            }
            else
            {
                material = ((material % enemy.SkinMaterials.Count) + enemy.SkinMaterials.Count) % enemy.SkinMaterials.Count;
            }
            var chosenMaterial = enemy.SkinMaterials[material];
            enemy.SetSkin(chosenMaterial);
        }
        enemy.SetMagic(isMagic);
        enemy.gameObject.transform.SetParent(container.transform);

        // Reset stats & state
        return enemy;
    }

    public void ReturnEnemy(Enemy enemy)
    {
        if (enemy.InUse)
        {
            enemy.StopAttacking();
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
}
