using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextUtil
{
    public static string TranslateTextKey(string key)
    {
        if (string.IsNullOrEmpty(key))
            return string.Empty;

        // TODO: Implement language translation and possibly a translated text database.
        return key;
    }
}
