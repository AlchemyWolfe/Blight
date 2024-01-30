using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Blight/MagicMaterials", fileName = "SO_MagicMaterials")]
public class MagicMaterialsSO : ScriptableObject
{
    [SerializeField]
    public List<Material> MagicMaterials;

    [SerializeField]
    public List<Material> WindMaterials;

    public Material GetMatchingWindMaterial(Material magicMaterial)
    {
        for (var i = 0; i < MagicMaterials.Count && i < WindMaterials.Count; ++i)
        {
            if (MagicMaterials[i].color == magicMaterial.color)
            {
                return WindMaterials[i];
            }
        }
        var iWind = Random.Range(0, WindMaterials.Count);
        return WindMaterials[iWind];
    }
}
