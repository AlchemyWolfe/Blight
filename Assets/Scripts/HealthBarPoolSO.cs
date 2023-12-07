using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(menuName = "Blight/HealthBarPool", fileName = "SO_HealthBarPool_")]
public class HealthBarPoolSO : ScriptableObject
{
    [SerializeField]
    public WorldHealthBar HealthBarPrefab;

    [SerializeField, HideInInspector]
    public ObjectPool<WorldHealthBar> HealthBarPool;

    [HideInInspector]
    public Canvas WorldCanvas;

    public void Initialize(Canvas worldCanvas)
    {
        WorldCanvas = worldCanvas;
        if (HealthBarPool == null)
        {
            HealthBarPool = new ObjectPool<WorldHealthBar>(OnCreateHealthBar, OnGetHealthBar, OnReleaseHealthBar, OnDestroyHealthBar, false, 10, 100);
        }
        HealthBarPool.Clear();
    }

    public WorldHealthBar CreateHealthBar(GameObject followTarget)
    {
        var healthBar = HealthBarPool.Get();
        healthBar.WorldCanvas = WorldCanvas;
        healthBar.FollowTarget = followTarget;
        return healthBar;
    }

    public void ReturnHealthBar(WorldHealthBar healthBar)
    {
        if (healthBar.InUse)
        {
            HealthBarPool.Release(healthBar);
        }
    }

    private WorldHealthBar OnCreateHealthBar()
    {
        var healthBar = GameObject.Instantiate(HealthBarPrefab);
        healthBar.Pool = this;
        return healthBar;
    }

    private void OnGetHealthBar(WorldHealthBar healthBar)
    {
        healthBar.gameObject.SetActive(true);
        healthBar.InUse = true;
    }

    private void OnReleaseHealthBar(WorldHealthBar healthBar)
    {
        healthBar.gameObject.SetActive(false);
        healthBar.transform.parent = null;
        healthBar.InUse = false;
    }

    private void OnDestroyHealthBar(WorldHealthBar healthBar)
    {
        Destroy(healthBar.gameObject);
    }
}
