using DG.Tweening;
using MalbersAnimations;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : BlightCreature
{
    //public List<Material> SkinMaterials;

    public WorldHealthBar HealthBar;
    [HideInInspector]
    public Action<Enemy> OnKilled;
    [HideInInspector]
    public Action<Enemy> OnKilledByPlayer;

    [HideInInspector]
    public EnemyDefinitionSO Pool;

    [HideInInspector]
    public bool IsBoss;
    [HideInInspector]
    public bool InUse;
    [HideInInspector]
    public bool IsDying { get; private set; }
    [HideInInspector]
    public AudioSource Audio;

    private Vector3 InputDirection;
    private ICharacterMove CharacterMove;
    private bool HasBeenInBounds;
    private WaveMovement MoveBehavior;
    private int MovementCount;
    private bool NegativeDot;

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
        gameObject.layer = 21;  // Enemy
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
        MovementCount = 0;
    }

    public void ChangeMoveBehavior(WaveMovement moveBehavior)
    {
        MoveBehavior = moveBehavior;
        var toPlayer = Tools.Player.transform.position - gameObject.transform.position;
        switch (moveBehavior)
        {
            case WaveMovement.Sit:
                ChangeDirection(Vector3.zero);
                break;
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
            case WaveMovement.CircleOnce:
                ChangeDirection(toPlayer.normalized);
                break;
        }
        if (moveBehavior == WaveMovement.CircleOnce)
        {
            NegativeDot = Vector3.Dot(toPlayer, Vector3.right) < 0;
        }
    }

    public void Flee()
    {
        var toPlayer = Tools.Player.transform.position - gameObject.transform.position;
        MoveBehavior = WaveMovement.AimedStrafe;
        ChangeDirection(-toPlayer.normalized);
    }

    public void ChangeDirection(Vector3 direction)
    {
        InputDirection = direction.normalized;
        if (CharacterMove != null)
        {
            CharacterMove.SetInputAxis(InputDirection);
        }
    }

    void Update()
    {
        if (transform.position.y < -50f)
        {
            Die();
        }

        AdjustPositionForGameBounds();
        UpdateMovementDirectrion();
    }

    public void AdjustPositionForGameBounds()
    {
        if (!HasBeenInBounds)
        {
            // We haven't been seen yet.
            if (Tools.IsPointInFrustrum(transform.position))
            {
                // Yay, we have been seen.
                HasBeenInBounds = true;
            }
            else if (!Tools.IsPointInBounds(transform.position))
            {
                // We've fallount out of game bounds, even though we haven't been seen yet.  Move to the edge.
                transform.position = Tools.GetPointWithinBounds(transform.position);
            }
        }
        else
        {
            if (!Tools.IsPointInBounds(transform.position))
            {
                // We just left game bounds.
                if (IsBoss)
                {
                    // Bosses do not fall off the map.  Move to the edge.
                    transform.position = Tools.GetPointWithinBounds(transform.position);
                }
                else
                {
                    // We will never be seen again.  Clean up.
                    Die();
                }
            }
        }

    }

    public void UpdateMovementDirectrion()
    {
        switch (MoveBehavior)
        {
            case WaveMovement.CircleOnce:
            case WaveMovement.Circling:
                var toPlayer = Tools.Player.transform.position - gameObject.transform.position;
                toPlayer.y = 0;
                var fromPlayer = Quaternion.Euler(0, 90, 0) * toPlayer.normalized;
                var target = Tools.Player.transform.position + (fromPlayer * 15f);
                var toTarget = target - gameObject.transform.position;
                toTarget.y = 0;
                ChangeDirection(toTarget.normalized);
                var forwardDot = Vector3.Dot(Tools.Player.transform.forward, gameObject.transform.forward);
                Wielder.SetSprint(forwardDot > 0);
                break;
            default:
                ChangeDirection(InputDirection);
                break;
        }
        if (MoveBehavior == WaveMovement.CircleOnce)
        {
            var toPlayer = Tools.Player.transform.position - gameObject.transform.position;
            if (MovementCount == 0 && toPlayer.magnitude > 5f)
            {
                ++MovementCount;
            }
            if (MovementCount >= 1)
            {
                var negativeDot = Vector3.Dot(toPlayer, Vector3.right) < 0;
                if (negativeDot != NegativeDot)
                {
                    // We've turned 180 degrees.  Have we circled once?
                    NegativeDot = negativeDot;
                    MovementCount++;
                    if (MovementCount > 3)
                    {
                        // Stop circling and just run off into the sunset.
                        MoveBehavior = WaveMovement.AimedStrafe;
                    }
                }
            }
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

    public void Die()
    {
        if (IsDying)
        {
            return;
        }
        IsDying = true;
        gameObject.layer = 19;  // Dead

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
