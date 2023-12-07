using UnityEngine;
using UnityEngine.Pool;

public class PrefabPool
{
    private GameObject Prefab { get; set; }
    private ObjectPool<GameObject> Pool { get; set; }
    private GameObject SafeParent { get; set; }

    public PrefabPool(GameObject prefab, GameObject safeParent, int defaultSize = 10, int maxSize = 100)
    {
        Prefab = prefab;
        SafeParent = safeParent;
        Pool = new ObjectPool<GameObject>(CreateGO, GetGO, ReleaseGO, DestroyGO, true, defaultSize, maxSize);
    }

    public GameObject Get()
    {
        return Pool.Get();
    }

    public void Release(GameObject go)
    {
        Pool.Release(go);
    }

    private GameObject CreateGO()
    {
        return GameObject.Instantiate(Prefab);
    }

    private void GetGO(GameObject go)
    {
        go.SetActive(true);
    }

    private void ReleaseGO(GameObject go)
    {
        if (SafeParent != null)
        {
            go.transform.parent = SafeParent.transform;
        }
        go.SetActive(false);
    }

    private void DestroyGO(GameObject go)
    {
#if UNITY_EDITOR
        GameObject.DestroyImmediate(go);
#else
        GameObject.Destroy(go);
#endif
    }
}