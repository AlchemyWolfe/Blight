using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Blight/SkinMaterials", fileName = "SO_SkinMaterials")]
public class SkinMaterialsSO : ScriptableObject
{
    [SerializeField]
    public List<Material> SkinMaterials;
}
