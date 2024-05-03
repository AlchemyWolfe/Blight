using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class Wave
{
    public WaveSO WaveDefinition;
    public GameObject EnemyContainer;
    public GameObject Player;
    public int WaveCount;
    public float WaveDuration;
    public IntPerLevel EnemyCount;
    public int KillCount;
    public List<Enemy> EnemyList;
    public GameOptionsSO Options;
    public GameSceneToolsSO Tools;
    public bool AllSpawned;
    public delegate void WaveCallback(Wave wave);
    public WaveCallback onAllEnemiesSpawned;
    public WaveCallback onWaveComplete;

    private Tween EnemySpawnTween;
    private int spawnDirection;
    private Vector2 spawnRange;
    private float spawnAngle;
    private float spawnStep;

    public Wave(WaveSO waveDefinition, GameObject enemyContainer, GameObject player, int waveCount, float waveDuration, GameOptionsSO options, GameSceneToolsSO tools, WaveCallback onAllEnemiesSpawned, WaveCallback onWaveComplete)
    {
        WaveDefinition = waveDefinition;
        EnemyContainer = enemyContainer;
        Player = player;
        WaveCount = waveCount;
        WaveDuration = waveDuration;
        Options = options;
        Tools = tools;
        var waveLevel = WaveCount - WaveDefinition.StartingWaveCount;
        EnemyCount = WaveDefinition.EnemyCount;
        EnemyCount.ScaleValues(1f);
        EnemyCount.SetLevel(waveLevel);
        this.onAllEnemiesSpawned = onAllEnemiesSpawned;
        this.onWaveComplete = onWaveComplete;
        AllSpawned = false;
        KillCount = 0;
    }

    public void OnDestroy()
    {
        EnemySpawnTween?.Kill();
    }

    public void SpawnEnemies(float duration)
    {
        if (EnemyCount.Value <= 0)
        {
            return;
        }

        switch (WaveDefinition.SpawnFormation)
        {
            case WaveFormation.HorizontalEdges:
                InitHorizontalEdgeEnemyPlacement();
                break;
            case WaveFormation.VerticalEdges:
                InitVerticalEdgeEnemyPlacement();
                break;
            case WaveFormation.HorizontalStream:
                InitHorizontalStreamEnemyPlacement();
                break;
            case WaveFormation.InwardRandom:
                InitInwardRandomEnemyPlacement();
                break;
            case WaveFormation.InwardSpiral:
                InitInwardSpiralEnemyPlacement();
                break;
        }

        var spawnRate = WaveDuration / EnemyCount.Value;
        var enemyDefinition = WaveDefinition.GetRandomEnemyDefinition();
        EnemySpawnTween?.Kill();
        var enemyIdx = 1;
        EnemySpawnTween = DOVirtual.DelayedCall(
                spawnRate,
                () =>
                {
                    var enemy = SpawnEnemy(enemyDefinition);
                    enemy.Initialize(1f, 1f);
                    switch (WaveDefinition.SpawnFormation)
                    {
                        case WaveFormation.HorizontalEdges:
                            PlaceHorizontalEdgeEnemy(enemy, enemyIdx);
                            break;
                        case WaveFormation.VerticalEdges:
                            PlaceVerticalEdgeEnemy(enemy, enemyIdx);
                            break;
                        case WaveFormation.HorizontalStream:
                            PlaceHorizontalStreamEnemy(enemy, enemyIdx);
                            break;
                        case WaveFormation.InwardRandom:
                            PlaceInwardRandomEnemy(enemy, enemyIdx);
                            break;
                        case WaveFormation.InwardSpiral:
                            PlaceInwardSpiralEnemy(enemy, enemyIdx);
                            break;
                    }
                    enemy.ChangeMoveBehavior(WaveDefinition.MovementBehavior);
                }
            )
            .SetLoops(EnemyCount.Value)
            .OnComplete(() =>
            {
                AllSpawned = true;
                onAllEnemiesSpawned?.Invoke(this);
            }
            )
            .OnKill(() => EnemySpawnTween = null);
    }

    private Enemy SpawnEnemy(EnemyDefinitionSO enemyDefinition, int material = -1, bool useSecondarySkin = false, bool isMagic = false, int extraType = -1)
    {
        if (EnemyList == null)
        {
            EnemyList = new List<Enemy>();
        }
        Enemy enemy = enemyDefinition.CreateEnemy(EnemyContainer, material, useSecondarySkin, isMagic, extraType);
        EnemyList.Add(enemy);
        enemy.Player = Player;
        enemy.Tools = Tools;
        enemy.OnKilled += OnKilledReceived;
        enemy.OnKilled += OnKilledByPlayerReceived;

        if (WaveDefinition.Weapon != null)
        {
            var weapon = WaveDefinition.Weapon.CreateWeapon(enemy, 1, 1);
        }

        return enemy;
    }

    private void OnKilledByPlayerReceived(Enemy enemy)
    {
        KillCount++;
        if (KillCount == EnemyCount.Value)
        {
            // TODO: Reward player for killing all enemies in a wave.
        }
    }

    private void OnKilledReceived(Enemy enemy)
    {
        enemy.OnKilled -= OnKilledReceived;
        EnemyList.Remove(enemy);
        if (AllSpawned && EnemyList.Count == 0)
        {
            onWaveComplete?.Invoke(this);
        }
    }


    private void InitHorizontalEdgeEnemyPlacement()
    {

    }

    private void PlaceHorizontalEdgeEnemy(Enemy enemy, int enemyIdx)
    {
        var center = Player.transform.position;
        if (Random.value < 0.5f)
        {
            var position = Tools.GetPointOnLeftEdge(center.y, WaveDefinition.OffScreenRadius);
            enemy.SetPositionOnGround(position);
        }
        else
        {
            var position = Tools.GetPointOnRightEdge(center.y, WaveDefinition.OffScreenRadius);
            enemy.SetPositionOnGround(position);
        }
    }

    private void InitVerticalEdgeEnemyPlacement()
    {

    }

    private void PlaceVerticalEdgeEnemy(Enemy enemy, int enemyIdx)
    {
        var center = Player.transform.position;
        if (Random.value < 0.5f)
        {
            var position = Tools.GetPointOnTopEdge(center.x, WaveDefinition.OffScreenRadius);
            enemy.SetPositionOnGround(position);
        }
        else
        {
            var position = Tools.GetPointOnBottomEdge(center.x, WaveDefinition.OffScreenRadius);
            enemy.SetPositionOnGround(position);
        }
    }

    private void InitHorizontalStreamEnemyPlacement()
    {
        spawnDirection = Random.Range(0, 2);
        var center = 0.5f + Random.Range(0f, 0.35f) - Random.Range(0f, 0.35f);
        spawnRange = new Vector2(center - 0.05f, center + 0.05f);
    }

    private void PlaceHorizontalStreamEnemy(Enemy enemy, int enemyIdx)
    {
        var center = Player.transform.position;
        if (spawnDirection == 0)
        {
            var position = Tools.GetPointOnLeftEdge(center.y, WaveDefinition.OffScreenRadius, spawnRange.x, spawnRange.y);
            enemy.SetPositionOnGround(position);
        }
        else
        {
            var position = Tools.GetPointOnRightEdge(center.y, WaveDefinition.OffScreenRadius, spawnRange.x, spawnRange.y);
            enemy.SetPositionOnGround(position);
        }
    }

    private void InitInwardRandomEnemyPlacement()
    {

    }

    private void PlaceInwardRandomEnemy(Enemy enemy, int enemyIdx)
    {
        var center = Player.transform.position;
        Ray2D[] edges = Tools.GetFrustrumEdges(center.y);
        Vector2 center2D = Tools.GetCenter(center.y);

        var angle = Random.value * 360f;

        var direction = Quaternion.Euler(0f, angle, 0f) * Vector3.right;
        var ray = new Ray2D(center2D, new Vector2(direction.x, direction.z));
        var distanceToEdge = Tools.GetTimeToNearestEdge(ray, edges) + WaveDefinition.OffScreenRadius;
        var position = center + (direction * distanceToEdge);

        enemy.SetPositionOnGround(position);
    }

    private void InitInwardSpiralEnemyPlacement()
    {
        spawnDirection = Random.Range(0, 2);
        spawnAngle = Random.value * 360f;
        spawnStep = Mathf.Max(12.5f, 360f / EnemyCount.Value);
    }

    private void PlaceInwardSpiralEnemy(Enemy enemy, int enemyIdx)
    {
        var center = Player.transform.position;
        Ray2D[] edges = Tools.GetFrustrumEdges(center.y);
        Vector2 center2D = Tools.GetCenter(center.y);

        var angle = spawnAngle + spawnStep * enemyIdx;

        var direction = Quaternion.Euler(0f, angle, 0f) * Vector3.right;
        var ray = new Ray2D(center2D, new Vector2(direction.x, direction.z));
        var distanceToEdge = Tools.GetTimeToNearestEdge(ray, edges) + WaveDefinition.OffScreenRadius;
        var position = center + (direction * distanceToEdge);

        enemy.SetPositionOnGround(position);
    }
}
