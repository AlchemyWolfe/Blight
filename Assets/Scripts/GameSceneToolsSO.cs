using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(menuName = "GameData/GameSceneTools", fileName = "SO_GameSceneTools")]
public class GameSceneToolsSO : ScriptableObject
{
    public BoxCollider InGameBounds;
    public float InGameBoundsCurbSize = 5f;
    public Terrain Ter;
    public Camera GameCamera;
    public Player Player;

    public System.Action OnShieldDown;

    public void AdjustInGameBounds(Camera camera)
    {
        GameCamera = camera;
        var corners = GetFrustrumCorners(InGameBounds.transform.position.y);
        var length = corners[3].x - corners[0].x + InGameBoundsCurbSize * 2f;
        var depth = corners[1].y - corners[0].y + InGameBoundsCurbSize * 2f;
        InGameBounds.size = new Vector3(length, InGameBounds.size.y, depth);
    }

    public Vector2 GetCenter(float y)
    {
        Ray centerRay = GameCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        return Get2DPointAtHeight(centerRay, y);
    }

    public Vector2[] GetFrustrumCorners(float y)
    {
        Ray bottomLeft = GameCamera.ViewportPointToRay(new Vector3(0, 0, 0));
        Ray topLeft = GameCamera.ViewportPointToRay(new Vector3(0, 1, 0));
        Ray topRight = GameCamera.ViewportPointToRay(new Vector3(1, 1, 0));
        Ray bottomRight = GameCamera.ViewportPointToRay(new Vector3(1, 0, 0));
        Vector2[] corners = new Vector2[4];
        corners[0] = Get2DPointAtHeight(bottomLeft, y);
        corners[1] = Get2DPointAtHeight(topLeft, y);
        corners[2] = Get2DPointAtHeight(topRight, y);
        corners[3] = Get2DPointAtHeight(bottomRight, y);
        return corners;
    }

    public Ray2D[] GetFrustrumEdges(float y)
    {
        Vector2[] corners = GetFrustrumCorners(y);
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

    public float GetTimeToNearestEdge(Ray2D ray, Ray2D[] edges)
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
