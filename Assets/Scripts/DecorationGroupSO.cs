using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Blight/DecorationGroup", fileName = "SO_DecorationGroup_")]
public class DecorationGroupSO : ScriptableObject
{
    public List<GameObject> common;
    public List<GameObject> rare;
    public List<GameObject> leaves;

    public float Density = 0.9f;
    [ShowIf("HasRare")]
    public float RareChance = 0.25f;
    public float MinScale = 0.9f;
    public float MaxScale = 1.1f;
    [ShowIf("HasLeaves")]
    public float LeafMinDistance = 0.1f;
    [ShowIf("HasLeaves")]
    public float LeafMaxDistance = 4f;

    private bool HasRare { get { return rare != null && rare.Count > 0; } }
    private bool HasLeaves { get { return leaves != null && leaves.Count > 0; } }

    public Decoration GetDecoration(Transform parent, Vector3 position)
    {
        var rand = Random.value;
        if (rand > Density)
        {
            return null;
        }
        rand = Random.value;
        GameObject mainPrefab = null;
        if (rand < RareChance && rare != null && rare.Count > 0)
        {
            // rare
            var idx = Random.Range(0, rare.Count);
            mainPrefab = rare[idx];
        }
        else if (common != null && common.Count > 0)
        {
            // common
            var idx = Random.Range(0, common.Count);
            mainPrefab = common[idx];
        }
        if (mainPrefab == null)
        {
            return null;
        }

        // Yay, we have an object.
        Decoration decoration = new Decoration();
        var yRotation = Random.Range(0f, 360f);
        var scale = Random.Range(MinScale, MaxScale);
        var mainGO = GameObject.Instantiate(mainPrefab, position, Quaternion.Euler(0f, yRotation, 0f));
        var posBeforeScale = mainGO.transform.position;
        mainGO.transform.localScale = mainGO.transform.localScale * scale;
        var posBeforeParent = mainGO.transform.position;
        mainGO.transform.parent = parent;
        decoration.AddObject(mainGO);

        if (leaves != null && leaves.Count > 0)
        {
            var leafCount = Random.Range(0, 3);
            for (var i = 0; i < leaves.Count; i++)
            {
                var idx = Random.Range(0, leaves.Count);
                var leafPrefab = leaves[idx];
                var offset = Random.insideUnitCircle * Random.Range(LeafMinDistance, LeafMaxDistance);
                yRotation = Random.Range(0f, 360f);
                var leaf = GameObject.Instantiate(leafPrefab,
                    new Vector3(position.x + offset.x, position.y, position.z + offset.y),
                    Quaternion.Euler(0f, yRotation, 0f));
                leaf.transform.localScale = leaf.transform.localScale * scale;
                leaf.transform.parent = mainGO.transform;
                decoration.AddObject(leaf);
            }
        }

        return decoration;
    }
}
