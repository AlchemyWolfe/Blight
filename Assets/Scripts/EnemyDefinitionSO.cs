using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(menuName = "Blight/EnemyDefinition", fileName = "SO_EnemyDefinition_")]
public class EnemyDefinitionSO : ScriptableObject
{
    [SerializeField]
    public Enemy EnemyPrefab;

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

    public Enemy CreateEnemy(GameObject container, int material = -1, bool isMagic = false)
    {
        var enemy = EnemyPool.Get();

        // Set the colors
        if (Materials != null && Materials.Count > 0)
        {
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
        enemy.SetMagic(isMagic);
        enemy.gameObject.transform.parent = container.transform;

        // Reset stats & state
        return enemy;
    }

    public void ReturnEnemy(Enemy enemy)
    {
        if (enemy.InUse)
        {
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
        enemy.InUse = true;
    }

    private void OnReleaseEnemy(Enemy enemy)
    {
        enemy.gameObject.SetActive(false);
        enemy.transform.parent = null;
        enemy.InUse = false;
    }

    private void OnDestroyEnemy(Enemy enemy)
    {
        Destroy(enemy.gameObject);
    }
}
