using System.Collections.Generic;

public class RandUtil
{
    public static T GetListItem<T>(System.Random rand, List<T> list)
    {
        if (list == null || list.Count == 0)
        {
            return default(T);
        }

        var randomIndex = rand.Next(list.Count);
        return list[randomIndex];
    }
}
