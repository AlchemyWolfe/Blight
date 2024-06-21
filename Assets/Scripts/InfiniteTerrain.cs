using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{
    public GameSceneToolsSO Tools;

    [Tooltip("Transform of the GameObject the terrain should follow.  I.E. the player's characterr.")]
    public Transform FollowTarget;

    [Tooltip("Disables all heightmap changes")]
    public bool DisableBumps;

    [Header("Perlin Land Shaping")]
    [Range(0f, 1f)]
    [Tooltip("Scale the actual height.")]
    public float HeightMultiplier = 0.005f;

    [Tooltip("Scale the heightmap changes.")]
    [Range(0.001f, 0.05f)]
    public float PerlinStretch = 0.03f;

    [Tooltip("Scale the changes for texture 1.")]
    [Range(0.001f, 0.5f)]
    public float SplatStretch1 = 0.02f;

    [Tooltip("Scale the changes for texture 1.")]
    [Range(0.001f, 0.5f)]
    public float SplatStretch2 = 0.01f;

    [Tooltip("Scale the changes for texture 1.")]
    [Range(0.001f, 0.5f)]
    public float SplatStretch3 = 0.015f;

    [Tooltip("Container for all decoration objects, just for organization.")]
    public GameObject DecorationContainer;
    private Transform DecorationTransform;

    [Tooltip("Groups of decorations that can make up the forest floor.")]
    public List<DecorationGroupSO> DecorationGroups;

    [Tooltip("Distance between generated Obstacles.")]
    public float ObstacleStep = 32f;
    public int ObstacleGridSize = 5;
    private Vector3Int obstacleGridPosition;

    [Tooltip("Trees & Rocks which can appear and must be destroyed or avoided.")]
    public List<DecorationGroupSO> ObstacleGroups;

    // The terrain we are moving.
    private Terrain Ter;
    // The TerrainData, which we will be mostly interacting with.
    private TerrainData Data;

    // Resolution of the height grid.
    private int MeshRez;
    // The height grid of the terrain.
    private float[,] Mesh;

    // Resolution of the splat grid.
    private int SplatRez;
    // The alphas for the textures on the terrain, in [X,Y,Layer].
    private float[,,] Splat;

    // How much to move the terrain so the FollowTarget is in the center.
    private float CenterOffset;
    private float TerrainWidth;

    // The amount to move by when we move.
    private float SplatStep = 1f;
    private int GridSize;
    private Vector3Int splatGridPosition;

    private DecorationGroupSO PrimaryDecorations;
    private DecorationGroupSO SecondaryDecorations;
    private Decoration[,] Decorations;
    private Decoration[,] Obstacles;
    private List<DestructibleDecoration> DestructibleObstacles;

    public void Initialize()
    {
        // Get our components and measurements.
        Ter = GetComponent<Terrain>();
        Tools.Ter = Ter;
        Data = Ter.terrainData;
        MeshRez = Data.heightmapResolution;
        Mesh = Data.GetHeights(0, 0, MeshRez, MeshRez);
        SplatRez = Data.alphamapWidth;
        Splat = new float[SplatRez, SplatRez, 3];
        CenterOffset = -Data.size.x / 2;
        TerrainWidth = Data.size.x;
        SplatStep = Data.terrainLayers[0].tileSize.x;
        splatGridPosition = Vector3Int.zero;
        GridSize = (int)(SplatRez / SplatStep);
        Decorations = new Decoration[GridSize, GridSize];
        Obstacles = new Decoration[ObstacleGridSize, ObstacleGridSize];
        DecorationTransform = DecorationContainer.transform;

        // Put the FollowTarget at the correct height.
        var adjustedPosition = FollowTarget.position;
        adjustedPosition.y = Ter.SampleHeight(adjustedPosition);
        FollowTarget.position = adjustedPosition;

        // Generate our initial values
        UpdateBumpies();
        SelectDecorations();
        DecorateAll();
        obstacleGridPosition = new Vector3Int((int)(FollowTarget.position.x / ObstacleStep), 0, (int)(FollowTarget.position.z / ObstacleStep));
        PopulateObstaclesAround(obstacleGridPosition.x, obstacleGridPosition.z);
        Tools.OnTerrainInitialized?.Invoke();
    }

    void Awake()
    {
        // I do this in Awake instead of Start because Start is not called in EditMode.
        Initialize();
    }

    private void OnDestroy()
    {
        for (var x = 0; x < GridSize; x++)
        {
            for (var z = 0; z < GridSize; z++)
            {
                var decoration = Decorations[x, z];
                if (decoration != null)
                {
                    decoration.Destroy();
                }
                Decorations[x, z] = null;
            }
        }
        for (var x = 0; x < ObstacleGridSize; x++)
        {
            for (var z = 0; z < ObstacleGridSize; z++)
            {
                var obstacle = Obstacles[x, z];
                if (obstacle != null)
                {
                    obstacle.Destroy();
                }
                Obstacles[x, z] = null;
            }
        }
    }

    void Update()
    {
        if (FollowTarget == null)
        {
            return;
        }
#if UNITY_EDITOR
        if (Mesh == null)
        {
            // I don't know why sometimes the editor loses Mesh.
            Initialize();
        }
#endif

        // Update our position
        // I convert to a scaled Vector3Int and back to make sure I am only moving by steps.
        var gridPos = new Vector3Int((int)(FollowTarget.position.x / SplatStep), 0, (int)(FollowTarget.position.z / SplatStep));
        if (gridPos != splatGridPosition)
        {
            Ter.transform.position = new Vector3((gridPos.x * SplatStep) + CenterOffset, 0, (gridPos.z * SplatStep) + CenterOffset);
            UpdateBumpies();
            MoveDecorationColumns(gridPos.x - splatGridPosition.x);
            MoveDecorationRows(gridPos.z - splatGridPosition.z);
            splatGridPosition = gridPos;
        }
        gridPos = new Vector3Int((int)(FollowTarget.position.x / ObstacleStep), 0, (int)(FollowTarget.position.z / ObstacleStep));
        if (gridPos != obstacleGridPosition)
        {
            PopulateObstaclesAround(gridPos.x, gridPos.z);
            obstacleGridPosition = gridPos;
        }
    }

    public void UpdateBumpies()
    {
        var size = Data.size;
        var scale = size / MeshRez;
        var terrainPosition = Ter.transform.position;

        // Set the height values for the Terrain.  Remember that x and z are flipped.
        if (DisableBumps)
        {
            for (var x = 0; x < MeshRez; x++)
            {
                for (var z = 0; z < MeshRez; z++)
                {
                    Mesh[x, z] = 0;
                }
            }
        }
        else
        {
            for (var x = 0; x < MeshRez; x++)
            {
                for (var z = 0; z < MeshRez; z++)
                {
                    var v = new Vector3(z * scale.z, 0f, x * scale.x) + terrainPosition;
                    Mesh[x, z] = Mathf.PerlinNoise(v.x * PerlinStretch, v.z * PerlinStretch) * HeightMultiplier;
                }
            }
        }
        Ter.terrainData.SetHeights(0, 0, Mesh);

        // Set the alpha values for the splat maps.  Remember that x and z are flipped.
        // The total of all layers must be <= 1f.
        scale.x = size.x / SplatRez;
        scale.z = size.z / SplatRez;
        for (var x = 0; x < SplatRez; x++)
        {
            for (var z = 0; z < SplatRez; z++)
            {
                var v = new Vector3(z * scale.z, 0f, x * scale.x) + terrainPosition;
                var topAlpha = Mathf.PerlinNoise(v.x * SplatStretch1, v.z * SplatStretch1);
                var midAlpha = Mathf.PerlinNoise(v.x * SplatStretch2, v.z * SplatStretch2);
                var bottomAlpha = Mathf.PerlinNoise(v.x * SplatStretch3, v.z * SplatStretch3) * 0.5f;
                var total = topAlpha + midAlpha + bottomAlpha;
                Splat[x, z, 0] = topAlpha / total;//>= midAlpha && topAlpha > bottomAlpha ? 1f : 0f;
                Splat[x, z, 1] = midAlpha / total;//>= topAlpha && midAlpha > bottomAlpha ? 1f : 0f;
                Splat[x, z, 2] = bottomAlpha / total;//>= midAlpha && bottomAlpha > topAlpha ? 1f : 0f;
            }
        }
        Data.SetAlphamaps(0, 0, Splat);
    }

    public void SelectDecorations()
    {
        var idx = Random.Range(0, DecorationGroups.Count);
        PrimaryDecorations = DecorationGroups[idx];
        idx = Random.Range(0, DecorationGroups.Count);
        SecondaryDecorations = DecorationGroups[idx];
    }

    public void DecorateAll()
    {
        var origin = Ter.transform.position;
        var halfStep = SplatStep / 2f;
        for (var x = 0; x < GridSize; x++)
        {
            for (var z = 0; z < GridSize; z++)
            {
                var decoration = Decorations[x, z];
                if (decoration != null)
                {
                    decoration.Destroy();
                    Decorations[x, z] = null;
                }
                var pos = new Vector3(
                    (x * SplatStep) + origin.x + Random.Range(-halfStep, halfStep),
                    0f,
                    (z * SplatStep) + origin.z + Random.Range(-halfStep, halfStep));
                var rand = Random.value;
                decoration = (rand < 0.25)
                    ? SecondaryDecorations.GetDecoration(DecorationTransform, pos)
                    : PrimaryDecorations.GetDecoration(DecorationTransform, pos);
                if (decoration != null)
                {
                    decoration.ReHeight(Ter);
                    Decorations[x, z] = decoration;
                }
            }
        }
    }

    public void MoveDecorationColumns(int dx)
    {
        // TODO: Add functionality for phasing out one decoration type for a new one.
        if (dx == 0)
        {
            return;
        }
        // TODO: Handle dx being > -1 or < 1.  Could be recursion.
        if (dx < 0)
        {
            for (var z = 0; z < GridSize; z++)
            {
                var decoration = Decorations[GridSize - 1, z];
                for (var x = GridSize - 2; x >= 0; x--)
                {
                    Decorations[x + 1, z] = Decorations[x, z];
                }
                Decorations[0, z] = decoration;
                if (decoration != null)
                {
                    decoration.MoveBy(-TerrainWidth, 0f, Ter);
                }
            }
        }
        else
        {
            for (var z = 0; z < GridSize; z++)
            {
                var decoration = Decorations[0, z];
                for (var x = 1; x < GridSize; x++)
                {
                    Decorations[x - 1, z] = Decorations[x, z];
                }
                Decorations[GridSize - 1, z] = decoration;
                if (decoration != null)
                {
                    decoration.MoveBy(TerrainWidth, 0f, Ter);
                }
            }
        }
    }

    public void MoveDecorationRows(int dz)
    {
        // TODO: Add functionality for phasing out one decoration type for a new one.
        if (dz == 0)
        {
            return;
        }
        // TODO: Handle dz being > -1 or < 1.  Could be recursion.
        if (dz < 0)
        {
            for (var x = 0; x < GridSize; x++)
            {
                var decoration = Decorations[x, GridSize - 1];
                for (var z = GridSize - 2; z >= 0; z--)
                {
                    Decorations[x, z + 1] = Decorations[x, z];
                }
                Decorations[x, 0] = decoration;
                if (decoration != null)
                {
                    decoration.MoveBy(0f, -TerrainWidth, Ter);
                }
            }
        }
        else
        {
            for (var x = 0; x < GridSize; x++)
            {
                var decoration = Decorations[x, 0];
                for (var z = 1; z < GridSize; z++)
                {
                    Decorations[x, z - 1] = Decorations[x, z];
                }
                Decorations[x, GridSize - 1] = decoration;
                if (decoration != null)
                {
                    decoration.MoveBy(0f, TerrainWidth, Ter);
                }
            }
        }
    }

    public void PopulateObstaclesAround(int centerX, int centerZ)
    {
        var center = new Vector3(obstacleGridPosition.x * ObstacleStep, 0f, obstacleGridPosition.z * ObstacleStep);
        var quarterStep = ObstacleStep / 4f;
        for (var x = -1; x <= 3; x++)
        {
            var gridX = x + centerX;
            var storeX = ((gridX % ObstacleGridSize) + ObstacleGridSize) % ObstacleGridSize; // % is remainder, not modulo.
            var worldX = gridX * ObstacleStep;
            for (var z = -1; z <= 3; z++)
            {
                if (!(x == -1 || x == 3 || z == -1 || z == 3))
                {
                    // Only do the borders.
                    continue;
                }
                var gridZ = z + centerZ;
                var storeZ = ((gridZ % ObstacleGridSize) + ObstacleGridSize) % ObstacleGridSize;
                var worldZ = gridZ * ObstacleStep;

                var pos = new Vector3(
                    worldX + Random.Range(-quarterStep, quarterStep),
                    0f,
                    worldZ + Random.Range(-quarterStep, quarterStep));

                var obstacle = Obstacles[storeX, storeZ];
                if (obstacle == null)
                {
                    var idx = Random.Range(0, ObstacleGroups.Count);
                    var obstacleGroup = ObstacleGroups[idx];
                    obstacle = obstacleGroup.GetDecoration(DecorationTransform, pos);
                    if (obstacle != null)
                    {
                        obstacle.ReHeight(Ter);
                    }
                    Obstacles[storeX, storeZ] = obstacle;
                }
                else
                {
                    // Only move if necessary.
                    var offset = obstacle.position - pos;
                    if (Mathf.Abs(offset.x) > quarterStep || Mathf.Abs(offset.z) > quarterStep)
                    {
                        obstacle.MoveTo(pos, Ter);
                    }
                }
            }
        }
    }

    public void AddDestructibles(Decoration decoration, bool isObstable = true)
    {
        var destructible = decoration.MainGO.GetComponent<DestructibleDecoration>();
        if (destructible != null)
        {
            destructible.OnKilled += isObstable ? OnObstacleKilledReceived : OnDecorationKilledReceived;
        }
        foreach (var leaf in decoration.Objects)
        {
            if (leaf != decoration.MainGO)
            {
                destructible = decoration.MainGO.GetComponent<DestructibleDecoration>();
                if (destructible != null)
                {
                    destructible.OnKilled += OnLeafKilledReceived;
                }
            }
        }
    }

    public void OnObstacleKilledReceived(DestructibleDecoration destructible)
    {
        destructible.OnKilled -= OnObstacleKilledReceived;
        for (var x = 0; x < ObstacleGridSize; x++)
        {
            for (var z = 0; z < ObstacleGridSize; z++)
            {
                var obstacle = Obstacles[x, z];
                if (destructible.decoration == obstacle)
                {
                    Obstacles[x, z] = null;
                }
            }
        }
        SpawnRewards(destructible);
    }

    public void OnDecorationKilledReceived(DestructibleDecoration destructible)
    {
        destructible.OnKilled -= OnDecorationKilledReceived;
        for (var x = 0; x < GridSize; x++)
        {
            for (var z = 0; z < GridSize; z++)
            {
                var decoration = Decorations[x, z];
                if (destructible.decoration == decoration)
                {
                    Decorations[x, z] = null;
                }
            }
        }
        SpawnRewards(destructible);
    }

    public void OnLeafKilledReceived(DestructibleDecoration destructible)
    {
        destructible.OnKilled -= OnLeafKilledReceived;
        destructible.decoration.RemoveLeaf(destructible.gameObject);
        SpawnRewards(destructible);
    }

    public void SpawnRewards(DestructibleDecoration destructible)
    {
        // TREASURE_REDUCTION_FACTOR = 0.1f ?
        // If MC needs health, try to spawn health, and drop chance of spawning anything else by TREASURE_REDUCTION_FACTOR.
        // If MC needs energy, try to spawn energy, and drop chance of spawning coins by TREASURE_REDUCTION_FACTOR.
        // Try to spawn coins.
    }
}
