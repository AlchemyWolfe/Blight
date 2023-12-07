using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class MultiPrefabPool
{
    private Dictionary<int, GameObject> Prefabs { get; set; } = new Dictionary<int, GameObject>();
    private Dictionary<int, ObjectPool<GameObject>> Pools { get; set; } = new Dictionary<int, ObjectPool<GameObject>>();
    private GameObject SafeParent { get; set; }
    private int DefaultSize { get; set; }
    private int MaxSize { get; set; }
    private int CurrentType { get; set; }

    public MultiPrefabPool(GameObject safeParent, int defaultSize = 10, int maxSize = 100)
    {
        SafeParent = safeParent;
        DefaultSize = defaultSize;
        MaxSize = maxSize;
    }

    public void InitPool(int type, GameObject prefab)
    {
        Prefabs.Add(type, prefab);
        Pools.Add(type, new ObjectPool<GameObject>(CreateGO, GetGO, ReleaseGO, DestroyGO, true, DefaultSize, MaxSize));
    }

    public GameObject GetPrefab(int type)
    {
        if (!Prefabs.ContainsKey(type))
        {
            return null;
        }
        return Prefabs[type];
    }

    public GameObject Get(int type, Transform parentTransform)
    {
        if (!Prefabs.ContainsKey(type))
        {
            return null;
        }
        CurrentType = type;
        var go = Pools[CurrentType].Get();
        go.transform.parent = parentTransform;
        return go;
    }

    public void Release(int type, GameObject go)
    {
        if (!Prefabs.ContainsKey(type))
        {
            return;
        }
        CurrentType = type;
        Pools[CurrentType].Release(go);
    }

    private GameObject CreateGO()
    {
        return GameObject.Instantiate(Prefabs[CurrentType]);
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