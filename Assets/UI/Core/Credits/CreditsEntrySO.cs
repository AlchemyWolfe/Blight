using Sirenix.OdinInspector;
using UnityEngine;

// I probably should have broken this up into multiple different scriptable objects.
// Since I am making entries for the packages I have imported, I have taken to creating the entry inside the package.
// That way, if I decide to delete the package, it will disappear from the credits.
// Hmm.  Should I make it skip those lines gracefully?  Or throw an error if I forget to update the credits?
[CreateAssetMenu(menuName = "Credits/Entry", fileName = "SO_Credits_Entry_")]
public class CreditsEntrySO : ScriptableObject
{
    public enum EntryType
    {
        Entry,
        Header,
        LargeHeader,
        Spacer,
        CoffeeEntry
    }

    [SerializeField]
    private EntryType _type;
    public EntryType Type => _type;

    [SerializeField]
    [ShowIf("IsHeader")]
    private string _headerKey;
    public string HeaderKey => _headerKey;

    [SerializeField]
    [ShowIf("ShowLeftText")]
    private string _leftKey;
    public string LeftKey => _leftKey;

    [SerializeField]
    [ShowIf("Type", Value = EntryType.Entry)]
    private string _rightKey;
    public string RightKey => _rightKey;

    [SerializeField]
    [ShowIf("Type", Value = EntryType.Spacer)]
    private GameObject _centerObject;
    public GameObject CenterObject => _centerObject;

    [SerializeField]
    [HideIf("Type", Value = EntryType.CoffeeEntry)]
    private GameObject _leftObject;
    public GameObject LeftObject => _leftObject;

    [SerializeField]
    private GameObject _rightObject;
    public GameObject RightObject => _rightObject;

    [SerializeField]
    [ShowIf("Type", Value = EntryType.CoffeeEntry)]
    private string _coffeeName;
    public string CoffeeName => _coffeeName;

#pragma warning disable IDE0051 // Remove unused private members.  These are used in Odin tags only.
    private bool IsHeader => Type is EntryType.Header or EntryType.LargeHeader;
    private bool ShowLeftText => Type == EntryType.Entry || Type == EntryType.CoffeeEntry;
#pragma warning restore IDE0051 // Remove unused private members.
}
