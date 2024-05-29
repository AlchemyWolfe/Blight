using System.Collections.Generic;
using UnityEngine;

public class Decoration
{
    public GameObject MainGO { get; private set; }
    public List<GameObject> Objects { get; private set; }
    public Vector3 position { get => MainGO != null ? MainGO.transform.position : Vector3.zero; }

    public void AddObject(GameObject go)
    {
        if (MainGO == null)
        {
            MainGO = go;
        }
        if (Objects == null)
        {
            Objects = new List<GameObject>();
        }
        Objects.Add(go);
    }

    public void MoveBy(float x, float z, Terrain terrain)
    {
        if (MainGO == null)
        {
            return;
        }
        var pos = MainGO.transform.position;
        pos.x += x;
        pos.z += z;
        MainGO.transform.position = pos;
        ReHeight(terrain);
    }

    public void MoveTo(Vector3 pos, Terrain terrain)
    {
        if (MainGO == null)
        {
            return;
        }
        MainGO.transform.position = pos;
        ReHeight(terrain);
    }

    public void ReHeight(Terrain terrain)
    {
        if (Objects == null)
        {
            return;
        }
        foreach (GameObject obj in Objects)
        {
            var pos = obj.transform.position;
            pos.y = terrain.SampleHeight(pos);
            obj.transform.position = pos;
        }
    }

    public void Destroy()
    {
        MainGO = null;
        if (Objects == null)
        {
            return;
        }
        for (var i = Objects.Count - 1; i >= 0; i--)
        {
            var obj = Objects[i];
#if UNITY_EDITOR
            GameObject.DestroyImmediate(obj);
#else
            GameObject.Destroy(obj);
#endif
        }
        Objects.Clear();
    }

    public void RemoveLeaf(GameObject leaf)
    {
        if (leaf == null || Objects == null)
        {
            return;
        }
        if (leaf == MainGO)
        {
            Debug.Log("RemoveLeaf: Attempting to remove MainGO: " + leaf.name);
        }
        else
        {
            Objects.Remove(leaf);
        }
    }
}
