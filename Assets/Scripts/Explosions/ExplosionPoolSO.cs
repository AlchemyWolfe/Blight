using MalbersAnimations;
using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(menuName = "Blight/ExplosionDefinition", fileName = "SO_ExplosionDefinition_")]
public class ExplosionPoolSO : ScriptableObject
{
    [SerializeField]
    public EnergyExplosion ExplosionPrefab;

    [SerializeField]
    public MagicMaterialsSO MagicMaterials;

    [SerializeField]
    public StatID Stat;

    [SerializeField, HideInInspector]
    public ObjectPool<EnergyExplosion> ExplosionPool;

    public void Initialize()
    {
        if (ExplosionPool == null)
        {
            ExplosionPool = new ObjectPool<EnergyExplosion>(OnCreateExplosion, OnGetExplosion, OnReleaseExplosion, OnDestroyExplosion, false, 10, 100);
        }
        ExplosionPool.Clear();
    }

    public EnergyExplosion CreateExplosion(GameObject attacker, Vector3 position, int level, Material magicMaterial)
    {
        var explosion = ExplosionPool.Get();
        explosion.Attacker = attacker;
        explosion.transform.position = position;
        explosion.WindMaterial = MagicMaterials.GetMatchingWindMaterial(magicMaterial);
        explosion.Level = level;
        explosion.Initialize();
        // Finally, be on the layer of my parent.
        explosion.gameObject.layer = attacker.gameObject.layer;
        return explosion;
    }

    public void ReturnExplosion(EnergyExplosion explosion)
    {
        if (explosion.InUse)
        {
            ExplosionPool.Release(explosion);
        }
    }

    private EnergyExplosion OnCreateExplosion()
    {
        var explosion = GameObject.Instantiate(ExplosionPrefab);
        explosion.Pool = this;
        explosion.Reset();
        return explosion;
    }

    private void OnGetExplosion(EnergyExplosion explosion)
    {
        explosion.gameObject.SetActive(true);
        explosion.Reset();
        explosion.InUse = true;
    }

    private void OnReleaseExplosion(EnergyExplosion explosion)
    {
        explosion.gameObject.SetActive(false);
        explosion.InUse = false;
    }

    private void OnDestroyExplosion(EnergyExplosion explosion)
    {
        Destroy(explosion.gameObject);
    }
}
