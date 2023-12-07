using UnityEngine;

public class MathUtil
{
    public static float GetRandomFloat(System.Random rand, float min, float max, float step = 0f)
    {
        var range = max - min;
        if (range == 0f)
        {
            return min;
        }

        if (step <= 0f)
        {
            var percent = rand.NextDouble();
            return (float)(percent * range) + min;
        }

        var availableSteps = (int)(range / step);
        var steps = rand.Next(availableSteps);
        return (float)(steps * step) + min;
    }

    public static float GetTimeProgress(float StartTime, float EndTime)
    {
        if (Time.time < StartTime)
        {
            return 0f;
        }
        if (Time.time > EndTime)
        {
            return 1f;
        }

        var progress = Time.time - StartTime;
        var duration = EndTime - StartTime;

        return duration > 0f ? progress / duration : 1f;
    }
}
