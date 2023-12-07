using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GameData/ListOfRandomText", fileName = "SO_RandomText_")]
public class RandomTextSO : ScriptableObject
{
    [SerializeField]
    private List<string> _textEntries;
    public List<string> TextEntries => _textEntries;

    public string GetRandomText()
    {
        if (TextEntries == null || TextEntries.Count == 0)
        {
            return string.Empty;
        }

        var i = Random.Range(0, TextEntries.Count);
        return TextEntries[i];
    }
}
