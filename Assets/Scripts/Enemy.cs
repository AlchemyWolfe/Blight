using DG.Tweening;
using MalbersAnimations;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : BlightCreature
{
    public List<Material> SkinMaterials;

    public WorldHealthBar HealthBar;
    [HideInInspector]
    public Action<Enemy> OnKilled;
    [HideInInspector]
    public Action<Enemy> OnKilledByPlayer;

    [HideInInspector]
    public EnemyDefinitionSO Pool;
    [HideInInspector]
    public GameObject Player;

    [HideInInspector]
    public bool IsBoss;
    [HideInInspector]
    public bool InUse;
    [HideInInspector]
    public bool IsDying { get; private set; }
    [HideInInspector]
    public GameSceneToolsSO Tools;
    [HideInInspector]
    public AudioSource Audio;

    private Vector3 InputDirection;
    private ICharacterMove CharacterMove;
    private bool HasBeenInBounds;
    private WaveMovement MoveBehavior;

    private void Start()
    {
        CharacterMove = GetComponent<ICharacterMove>();
        CharacterMove.SetInputAxis(InputDirection);
    }

    public void Reset()
    {
        IsDying = false;
        HasBeenInBounds = false;
        IsBoss = false;
        if (Audio == null)
        {
            Audio = gameObject.AddComponent<AudioSource>();
        }
    }

    public void Initialize(float health, float scale)
    {
        InitializeWeapons();
        if (gameObject.TryGetComponent<Stats>(out var stats))
        {
            var stat = stats.Stat_Get("Health");
            stat.SetMAX(1f);
            stat.Active = true;
            stat.Value = 1f;
        }
        gameObject.transform.localScale = new Vector3(scale, scale, scale);
        DOVirtual.DelayedCall(1f, StartAttacking);
    }

    public void ChangeMoveBehavior(WaveMovement moveBehavior)
    {
        MoveBehavior = moveBehavior;
        var toPlayer = Player.transform.position - gameObject.transform.position;
        switch (moveBehavior)
        {
            case WaveMovement.HorizontalStrafe:
                var dx = toPlayer.x > 0f ? 1f : -1f;
                ChangeDirection(new Vector3(dx, 0f, 0f));
                break;
            case WaveMovement.VerticalStrafe:
                var dz = toPlayer.z > 0f ? 1f : -1f;
                ChangeDirection(new Vector3(0f, 0f, dz));
                break;
            case WaveMovement.AimedStrafe:
            case WaveMovement.Circling:
                ChangeDirection(toPlayer.normalized);
                break;
        }
    }

    public void ChangeDirection(Vector3 direction)
    {
        InputDirection = direction.normalized;
        if (CharacterMove != null)
        {
            CharacterMove.SetInputAxis(InputDirection);
        }
    }

    public void SetPositionOnGround(Vector3 position)
    {
        position.y = Tools.Ter.SampleHeight(position);
        gameObject.transform.position = position;
    }

    public void SetPositionOnGround(Vector2 position)
    {
        SetPositionOnGround(new Vector3(position.x, Player.transform.position.y, position.y));
    }

    void Update()
    {
        if (transform.position.y < -50f)
        {
            Die();
        }

        switch (MoveBehavior)
        {
            case WaveMovement.Circling:
                // everyone circles around the player for now
                var toPlayer = Player.transform.position - gameObject.transform.position;
                toPlayer.y = 0;
                var fromPlayer = Quaternion.Euler(0, 90, 0) * toPlayer.normalized;
                var target = Player.transform.position + (fromPlayer * 15f);
                var toTarget = target - gameObject.transform.position;
                toTarget.y = 0;
                ChangeDirection(toTarget.normalized);
                break;
            default:
                ChangeDirection(InputDirection);
                break;
        }
        
    }

    public void OnDamagReceived(float value)
    {
        //Debug.Log(gameObject.name + " took " + value + " damage.");
    }

    public void OnHealthChanged(float value)
    {
        //Debug.Log(gameObject.name + " is at " + value + "HP.");
        if (value <= 0.1f)
        {
            OnKilledByPlayer?.Invoke(this);
            Die();
        }
    }

    public void OnHealthPercentChanged(float value)
    {
        if (HealthBar != null)
        {
            HealthBar.HealthPercent = value;
            if (value >= 1f)
            {
                HealthBar.ReturnToPool();
                HealthBar = null;
            }
        }
        else if (Pool != null && value > 0f && value < 1f)
        {
            var healthBarPool = Pool.HealthBarPool;
            if (healthBarPool != null)
            {
                HealthBar = healthBarPool.CreateHealthBar(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Tools && other == Tools.InGameBounds)
        {
            HasBeenInBounds = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (Tools && other == Tools.InGameBounds && HasBeenInBounds)
        {
            if (IsBoss)
            {
                // Bosses do not fall off the map.
                var center = Tools.InGameBounds.center;
                var size = Tools.InGameBounds.size;
                var position = transform.position;
                position.x = Mathf.Clamp(position.x, center.x - size.x, center.x + size.x);
                position.z = Mathf.Clamp(position.z, center.z - size.z, center.z + size.z);
                position.y = Tools.Ter.SampleHeight(position);
                transform.position = position;
            }
            else
            {
                Die();
            }
        }
    }

    public void Die()
    {
        if (IsDying)
        {
            return;
        }
        IsDying = true;

        if (HealthBar != null)
        {
            HealthBar.ReturnToPool();
        }

        OnKilled?.Invoke(this);
        if (Pool != null)
        {
            Pool.ReturnEnemy(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
