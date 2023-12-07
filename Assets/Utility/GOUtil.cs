using UnityEngine;

public class GOUtil
{
    public static GameObject FindOrCreateChild(GameObject parent, string name)
    {
        GameObject go = null;
        var childTransform = parent.transform.Find(name);
        if (childTransform != null)
        {
            go = childTransform.gameObject;
        }
        else
        {
            go = new GameObject(name);
            go.transform.parent = parent.transform;
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
        }

        return go;
    }
}
