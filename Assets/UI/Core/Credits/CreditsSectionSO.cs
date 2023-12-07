using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Credits/Section", fileName = "SO_Credits_Section_")]
public class CreditsSectionSO : ScriptableObject
{
    [SerializeField]
    private List<CreditsEntrySO> _entries;
    public List<CreditsEntrySO> Entries => _entries;
}
