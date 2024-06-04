using UnityEngine;

//[CreateAssetMenu(menuName = "GameData/GameSceneTools", fileName = "SO_GameSceneTools")]
public class GameSceneToolsSO : ScriptableObject
{
    public Terrain Ter;
    public Camera GameCamera;
    public Player Player;
    public bool IsPlayingGame;

    public Vector2[] FrustrumCorners;
    public Ray2D[] FrustrumEdges;
    public Vector2[] BoundsCorners;
    public Ray2D[] BoundsEdges;

    public System.Action OnTerrainInitialized;
    public System.Action OnShieldDown;
    public System.Action OnGameStart;
    public System.Action OnGameOver;
    public System.Action OnGameClose;

    public float boundsBuffer = 5f;

    public void UpdateFrustrum(float y)
    {
        Ray bottomLeft = GameCamera.ViewportPointToRay(new Vector3(0, 0, 0));
        Ray topLeft = GameCamera.ViewportPointToRay(new Vector3(0, 1, 0));
        Ray topRight = GameCamera.ViewportPointToRay(new Vector3(1, 1, 0));
        Ray bottomRight = GameCamera.ViewportPointToRay(new Vector3(1, 0, 0));
        FrustrumCorners = new Vector2[4];
        FrustrumCorners[0] = Get2DPointAtHeight(bottomLeft, y);
        FrustrumCorners[1] = Get2DPointAtHeight(topLeft, y);
        FrustrumCorners[2] = Get2DPointAtHeight(topRight, y);
        FrustrumCorners[3] = Get2DPointAtHeight(bottomRight, y);
        FrustrumEdges = new Ray2D[4];
        FrustrumEdges[0] = new Ray2D(FrustrumCorners[0], FrustrumCorners[1] - FrustrumCorners[0]);
        FrustrumEdges[1] = new Ray2D(FrustrumCorners[1], FrustrumCorners[2] - FrustrumCorners[1]);
        FrustrumEdges[2] = new Ray2D(FrustrumCorners[2], FrustrumCorners[3] - FrustrumCorners[2]);
        FrustrumEdges[3] = new Ray2D(FrustrumCorners[3], FrustrumCorners[0] - FrustrumCorners[3]);
        BoundsCorners = new Vector2[4];
        BoundsCorners[0] = new Vector2(FrustrumCorners[0].x - boundsBuffer, FrustrumCorners[0].y - boundsBuffer);
        BoundsCorners[1] = new Vector2(FrustrumCorners[1].x - boundsBuffer, FrustrumCorners[1].y + boundsBuffer);
        BoundsCorners[2] = new Vector2(FrustrumCorners[2].x + boundsBuffer, FrustrumCorners[2].y + boundsBuffer);
        BoundsCorners[3] = new Vector2(FrustrumCorners[3].x + boundsBuffer, FrustrumCorners[3].y - boundsBuffer);
        BoundsEdges = new Ray2D[4];
        BoundsEdges[0] = new Ray2D(BoundsCorners[0], BoundsCorners[1] - BoundsCorners[0]);
        BoundsEdges[1] = new Ray2D(BoundsCorners[1], BoundsCorners[2] - BoundsCorners[1]);
        BoundsEdges[2] = new Ray2D(BoundsCorners[2], BoundsCorners[3] - BoundsCorners[2]);
        BoundsEdges[3] = new Ray2D(BoundsCorners[3], BoundsCorners[0] - BoundsCorners[3]);
    }

    public bool IsPointInFrustrum(Vector3 point)
    {
        // Note: We are assuming the camera never rotates, and the frustrum top and bottom are horizontal lines (dz = 0)
        if (point.z > FrustrumCorners[1].y || point.z < FrustrumCorners[0].y)
        {
            // Above topLeft or below bottom y
            return false;
        }
        var point2d = new Vector2(point.x, point.z);
        var toPoint = point2d - FrustrumCorners[0];  // from bottom left to point
        var edgeNormal = new Vector2(-FrustrumEdges[0].direction.y, FrustrumEdges[0].direction.x);
        if (Vector2.Dot(edgeNormal, toPoint) > 0)
        {
            return false;
        }
        toPoint = point2d - FrustrumCorners[2];
        edgeNormal = new Vector2(-FrustrumEdges[2].direction.y, FrustrumEdges[2].direction.x);
        if (Vector2.Dot(edgeNormal, toPoint) > 0)
        {
            return false;
        }
        return true;
    }

    public bool IsPointInBounds(Vector3 point)
    {
        // Note: We are assuming the camera never rotates, and the frustrum top and bottom are horizontal lines (dz = 0)
        if (point.z > BoundsCorners[1].y || point.z < BoundsCorners[0].y)
        {
            // Above topLeft or below bottom y
            return false;
        }
        var point2d = new Vector2(point.x, point.z);
        var toPoint = point2d - BoundsCorners[0];  // from bottom left to point
        var edgeNormal = new Vector2(-BoundsEdges[0].direction.y, BoundsEdges[0].direction.x);
        if (Vector2.Dot(edgeNormal, toPoint) > 0)
        {
            return false;
        }
        toPoint = point2d - BoundsCorners[2];
        edgeNormal = new Vector2(-BoundsEdges[2].direction.y, BoundsEdges[2].direction.x);
        if (Vector2.Dot(edgeNormal, toPoint) > 0)
        {
            return false;
        }
        return true;
    }

    public Vector3 GetPointWithinBounds(Vector3 point)
    {
        // Check top
        if (point.z > BoundsCorners[1].y)
        {
            point.x = Mathf.Clamp(point.x, BoundsCorners[1].x, BoundsCorners[2].x);
            point.z = BoundsCorners[1].y;
            return point;
        }
        // Check bottom
        if (point.z < BoundsCorners[0].y)
        {
            point.x = Mathf.Clamp(point.x, BoundsCorners[0].x, BoundsCorners[3].x);
            point.z = BoundsCorners[0].y;
            return point;
        }
        // Check left edge
        if (point.x < BoundsCorners[0].x)
        {
            var origin = BoundsEdges[0].origin;
            var slope = BoundsEdges[0].direction.y / BoundsEdges[0].direction.x;
            point.x = (point.y - origin.y + (slope * origin.x)) / slope;
            return point;
        }
        // Check right edge
        if (point.x > BoundsCorners[3].x)
        {
            var origin = BoundsEdges[2].origin;
            var slope = BoundsEdges[2].direction.y / BoundsEdges[2].direction.x;
            point.x = (point.y - origin.y + (slope * origin.x)) / slope;
            return point;
        }
        return point;
    }

    private Vector2 Get2DPointAtHeight(Ray ray, float height)
    {
        var point = ray.origin + ((ray.origin.y - height) / -ray.direction.y * ray.direction);
        return new Vector2(point.x, point.z);
    }

    public Vector2 GetPointOnLeftEdge(float padding, float min = 0.1f, float max = 0.9f)
    {
        var start = FrustrumCorners[0];
        var travel = FrustrumCorners[1] - FrustrumCorners[0];
        var distance = Random.Range(min, max);
        var point = start + (travel * distance);
        point.x -= padding;
        return point;
    }

    public Vector2 GetPointOnRightEdge(float padding, float min = 0.1f, float max = 0.9f)
    {
        var start = FrustrumCorners[2];
        var travel = FrustrumCorners[3] - FrustrumCorners[2];
        var distance = Random.Range(min, max);
        var point = start + (travel * distance);
        point.x += padding;
        return point;
    }

    public Vector2 GetPointOnTopEdge(float padding, float min = 0.1f, float max = 0.9f)
    {
        var start = FrustrumCorners[2];
        var travel = FrustrumCorners[1] - FrustrumCorners[2];
        var distance = Random.Range(min, max);
        var point = start + (travel * distance);
        point.y += padding;
        return point;
    }

    public Vector2 GetPointOnBottomEdge(float padding, float min = 0.1f, float max = 0.9f)
    {
        var start = FrustrumCorners[0];
        var travel = FrustrumCorners[3] - FrustrumCorners[0];
        var distance = Random.Range(min, max);
        var point = start + (travel * distance);
        point.y -= padding;
        return point;
    }

    public Vector2 GetPointOnRandomEdge(float padding, float min = 0.1f, float max = 0.9f)
    {
        var edge = Random.Range(0, 4);
        switch (edge)
        {
            case 0:
                return GetPointOnLeftEdge(padding, min, max);
            //break;
            case 1:
                return GetPointOnRightEdge(padding, min, max);
            //break;
            case 2:
                return GetPointOnTopEdge(padding, min, max);
                //break;
        }
        return GetPointOnBottomEdge(padding, min, max);
    }

    public Vector3 GetPointOnFrustrumEdge(float angle, float offScreenRadius)
    {
        var center = Player.transform.position;
        Vector2 center2D = new Vector2(center.x, center.z);
        var direction = Quaternion.Euler(0f, angle, 0f) * Vector3.right;
        var ray = new Ray2D(center2D, new Vector2(direction.x, direction.z));
        var distanceToEdge = GetTimeToNearestFrustrumEdge(ray) + offScreenRadius;
        var position = center + (direction * distanceToEdge);

        return position;
    }

    public float GetTimeToNearestFrustrumEdge(Ray2D ray)
    {
        var minTime = float.MaxValue;
        for (int i = 0; i < 4; i++)
        {
            var time = TimeToIntersect(ray, FrustrumEdges[i]);
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
