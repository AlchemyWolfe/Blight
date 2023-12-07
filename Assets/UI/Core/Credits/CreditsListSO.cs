using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Credits/List", fileName = "SO_Credits_List")]
public class CreditsListSO : ScriptableObject
{
    [SerializeField]
    private List<CreditsSectionSO> _sections;
    public List<CreditsSectionSO> Sections => _sections;
}
