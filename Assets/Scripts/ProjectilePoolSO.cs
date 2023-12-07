using MalbersAnimations;
using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(menuName = "Blight/ProjectileDefinition", fileName = "SO_ProjectileDefinition_")]
public class ProjectilePoolSO : ScriptableObject
{
    [SerializeField]
    public Projectile ProjectilePrefab;

    [SerializeField]
    public StatID Stat;

    [SerializeField]
    [Tooltip("Damage the projectile does.")]
    public FloatPerLevel Damage;

    [SerializeField]
    [Tooltip("Speed the projectile moves.")]
    public FloatPerLevel Velocity;

    [SerializeField]
    [Tooltip("Size multipler of the projectile.")]
    public FloatPerLevel Size;

    [SerializeField]
    [Tooltip("Maximum distance the projectile remains active.")]
    public float Lifespan = 50f;

    [SerializeField, HideInInspector]
    public ObjectPool<Projectile> ProjectilePool;

    private void OnValidate()
    {
        Damage.SetMinMax(0f, 1000f);
        Velocity.SetMinMax(0.01f, 1000f);
        Size.SetMinMax(0.01f, 1000f);
    }

    public void Initialize()
    {
        if (ProjectilePool == null)
        {
            ProjectilePool = new ObjectPool<Projectile>(OnCreateProjectile, OnGetProjectile, OnReleaseProjectile, OnDestroyProjectile, false, 10, 100);
        }
        ProjectilePool.Clear();
    }

    public Projectile CreateProjectile(GameObject attacker, Transform parent, Vector3 position, Vector3 forward, float damage, float velocity, float size, float lifespan)
    {
        var projectile = ProjectilePool.Get();
        projectile.Attacker = attacker;
        projectile.transform.localScale = ProjectilePrefab.transform.localScale * size;
        projectile.transform.position = position;
        projectile.transform.forward = forward;
        projectile.transform.SetParent(parent);
        projectile.Damage = damage;
        projectile.Velocity = velocity;
        projectile.Lifespan = lifespan;
        // Finally, be on the layer of my parent.
        projectile.gameObject.layer = parent.gameObject.layer;
        return projectile;
    }

    public void ReturnProjectile(Projectile projectile)
    {
        if (projectile.InUse)
        {
            ProjectilePool.Release(projectile);
        }
    }

    private Projectile OnCreateProjectile()
    {
        var projectile = GameObject.Instantiate(ProjectilePrefab);
        projectile.Pool = this;
        projectile.Reset();
        return projectile;
    }

    private void OnGetProjectile(Projectile projectile)
    {
        projectile.gameObject.SetActive(true);
        projectile.Reset();
        projectile.InUse = true;
    }

    private void OnReleaseProjectile(Projectile projectile)
    {
        projectile.gameObject.SetActive(false);
        projectile.InUse = false;
    }

    private void OnDestroyProjectile(Projectile projectile)
    {
        Destroy(projectile.gameObject);
    }
}
