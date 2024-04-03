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
    public List<Enemy> EnemyList;
    public bool WaveComplete { get { return EnemyList != null && EnemyList.Count == 0; } }
    public GameOptionsSO Options;
    public GameSceneToolsSO Tools;

    private Tween EnemySpawnTween;

    public Wave(WaveSO waveDefinition, GameObject enemyContainer, GameObject player, int waveCount, float waveDuration, GameOptionsSO options, GameSceneToolsSO tools)
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
                SpawnHorizontalStrafeEnemies();
                break;
            case WaveFormation.InwardRing:
                SpawnInwardRingEnemies();
                break;
            case WaveFormation.InwardRing_All:
                SpawnAllEnemies();
                break;
        }
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
        return enemy;
    }

    private void OnKilledReceived(Enemy enemy)
    {
        enemy.OnKilled -= OnKilledReceived;
        EnemyList.Remove(enemy);
    }

    private void SpawnHorizontalStrafeEnemies()
    {
        var spawnRate = WaveDuration / EnemyCount.Value;
        var enemyDefinition = WaveDefinition.GetRandomEnemyDefinition();
        EnemySpawnTween = DOVirtual.DelayedCall(
                spawnRate,
                () =>
                {
                    var enemy = SpawnEnemy(enemyDefinition);
                    enemy.Initialize(1f, 1f);
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
                    enemy.ChangeMoveBehavior(WaveMovement.HorizontalStrafe);
                }
            )
            .SetLoops(EnemyCount.Value)
            .OnKill(() => EnemySpawnTween = null);
    }

    private void SpawnInwardRingEnemies()
    {
        var center = Player.transform.position;
        Ray2D[] edges = Tools.GetFrustrumEdges(center.y);
        Vector2 center2D = Tools.GetCenter(center.y);

        var enemyDefinition = WaveDefinition.GetRandomEnemyDefinition();
        var angleStep = 360f / EnemyCount.Value;
        var angle = Random.value * 360f;
        for (int i = 0; i < EnemyCount.Value; i++)
        {
            angle += angleStep;
            var direction = Quaternion.Euler(0f, angle, 0f) * Vector3.right;
            var ray = new Ray2D(center2D, new Vector2(direction.x, direction.z));
            var distanceToEdge = Tools.GetTimeToNearestEdge(ray, edges) + WaveDefinition.OffScreenRadius;
            var position = center + (direction * distanceToEdge);
            position.y = Tools.Ter.SampleHeight(position);

            var enemy = SpawnEnemy(enemyDefinition, i);
            enemy.Initialize(1f, 1f);
            enemy.SetPositionOnGround(position);
            enemy.ChangeMoveBehavior(WaveMovement.Circling);
        }
    }

    private void SpawnAllEnemies()
    {
        var center = Player.transform.position;
        Ray2D[] edges = Tools.GetFrustrumEdges(center.y);
        Vector2 center2D = Tools.GetCenter(center.y);

        EnemyCount.Value = WaveDefinition.EnemyDefinitions.Count;
        var angleStep = 360f / EnemyCount.Value;
        var angle = Random.value * 360f;
        for (int i = 0; i < EnemyCount.Value; i++)
        {
            angle += angleStep;
            var direction = Quaternion.Euler(0f, angle, 0f) * Vector3.right;
            var ray = new Ray2D(center2D, new Vector2(direction.x, direction.z));
            var distanceToEdge = Tools.GetTimeToNearestEdge(ray, edges) + WaveDefinition.OffScreenRadius;
            var position = center + (direction * distanceToEdge);
            position.y = Tools.Ter.SampleHeight(position);

            var enemy = SpawnEnemy(WaveDefinition.EnemyDefinitions[i], i);
            enemy.Initialize(1f, 1f);
            enemy.SetPositionOnGround(position);
            enemy.ChangeMoveBehavior(WaveMovement.Circling);
        }
    }
}
