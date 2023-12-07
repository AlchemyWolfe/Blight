using UnityEngine;

public class Wave
{
    public WaveSO WaveDefinition;
    public Terrain Ter;
    public GameObject EnemyContainer;
    public GameObject Player;
    public int WaveCount;
    public int WaveLevel;

    public void SpawnEnemies()
    {
        switch (WaveDefinition.SpawnFormation)
        {
            case WaveFormation.InwardRing:
                SpawnInwardRingEnemies();
                break;
        }

    }

    private void SpawnInwardRingEnemies()
    {
        var center = Player.transform.position;
        Ray2D[] edges = GetFrustrumEdges(center.y);
        Vector2 center2D = GetCenter(center.y);

        var enemyCount = WaveDefinition.GetEnemyCount(WaveCount);
        var enemyDefinition = WaveDefinition.GetRandomEnemyDefinition();
        var angleStep = 360f / enemyCount;
        var angle = Random.value * 360f;
        for (int i = 0; i < enemyCount; i++)
        {
            angle += angleStep;
            var direction = Quaternion.Euler(0f, angle, 0f) * Vector3.right;
            var ray = new Ray2D(center2D, new Vector2(direction.x, direction.z));
            var distanceToEdge = GetTimeToNearestEdge(ray, edges) + WaveDefinition.OffScreenRadius;
            var position = center + (direction * distanceToEdge);
            position.y = Ter.SampleHeight(position) + 0.1f;
            var enemy = enemyDefinition.CreateEnemy(EnemyContainer, i);
            enemy.ChangeDirection(-direction);
            enemy.gameObject.transform.position = position;
        }
    }

    private Vector2 GetCenter(float y)
    {
        Camera camera = Camera.main;
        Ray centerRay = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        return Get2DPointAtHeight(centerRay, y);
    }

    private Ray2D[] GetFrustrumEdges(float y)
    {
        Camera camera = Camera.main;
        Ray bottomLeft = camera.ViewportPointToRay(new Vector3(0, 0, 0));
        Ray topLeft = camera.ViewportPointToRay(new Vector3(0, 1, 0));
        Ray topRight = camera.ViewportPointToRay(new Vector3(1, 1, 0));
        Ray bottomRight = camera.ViewportPointToRay(new Vector3(1, 0, 0));
        Vector2[] corners = new Vector2[4];
        corners[0] = Get2DPointAtHeight(bottomLeft, y);
        corners[1] = Get2DPointAtHeight(topLeft, y);
        corners[2] = Get2DPointAtHeight(topRight, y);
        corners[3] = Get2DPointAtHeight(bottomRight, y);
        Ray2D[] edges = new Ray2D[4];
        edges[0] = new Ray2D(corners[0], corners[1] - corners[0]);
        edges[1] = new Ray2D(corners[1], corners[2] - corners[1]);
        edges[2] = new Ray2D(corners[2], corners[3] - corners[2]);
        edges[3] = new Ray2D(corners[3], corners[0] - corners[3]);
        return edges;
    }

    private Vector2 Get2DPointAtHeight(Ray ray, float height)
    {
        var point = ray.origin + ((ray.origin.y - height) / -ray.direction.y * ray.direction);
        return new Vector2(point.x, point.z);
    }

    private float GetTimeToNearestEdge(Ray2D ray, Ray2D[] edges)
    {
        var minTime = float.MaxValue;
        for (int i = 0; i < edges.Length; i++)
        {
            var time = TimeToIntersect(ray, edges[i]);
            minTime = Mathf.Min(minTime, time);
        }
        return minTime;
    }

    // Function to calculate the distance between intersecting rays
    public float TimeToIntersect(Ray2D ray1, Ray2D ray2)
    {
        // Check if the rays are parallel (no intersection)
        float cross = (ray1.direction.x * ray2.direction.y) - (ray1.direction.y * ray2.direction.x);
        if (Mathf.Approximately(cross, 0f))
        {
            // Rays are parallel, you may want to handle this case differently
            return float.MaxValue;
        }

        // Calculate the distance between the rays
        Vector2 delta = ray2.origin - ray1.origin;
        float t1 = ((delta.x * ray2.direction.y) - (delta.y * ray2.direction.x)) / cross;
        if (t1 >= 0)
        {
            // Time > 0 means we are going in the right direction.
            return t1;
        }

        // The segment is behind us.
        return float.MaxValue;
    }
}