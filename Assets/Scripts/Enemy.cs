using DG.Tweening;
using MalbersAnimations;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : BlightCreature
{
    public SkinnedMeshRenderer Skin;
    public SkinnedMeshRenderer Magic;
    public SkinnedMeshRenderer Secondary;
    public List<Material> SkinMaterials;

    public WorldHealthBar HealthBar;
    [HideInInspector]
    public Action<Enemy> OnKilled;


    [HideInInspector]
    public EnemyDefinitionSO Pool;
    [HideInInspector]
    public GameObject Player;

    [HideInInspector]
    public bool InUse;
    [HideInInspector]
    public bool IsDying { get; private set; }

    private Vector3 InputDurection;
    private ICharacterMove CharacterMove;

    private void Start()
    {
        CharacterMove = GetComponent<ICharacterMove>();
        CharacterMove.SetInputAxis(InputDurection);
    }

    public void Reset()
    {
        IsDying = false;
    }

    public void Initialize(float health, float scale)
    {
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

    public void SetSkin(Material material)
    {
        if (Skin == null)
        {
            Skin = GetComponent<SkinnedMeshRenderer>();
        }
        if (Skin != null)
        {
            Skin.material = material;
        }
    }

    public void SetMagic(bool isMagic)
    {
        Magic.enabled = isMagic;
    }

    public void ChangeDirection(Vector3 newDirection)
    {
        InputDurection = newDirection;
        if (CharacterMove != null)
        {
            CharacterMove.SetInputAxis(InputDurection);
        }
    }

    void Update()
    {
        // everyone circles around the player for now
        var toPlayer = Player.transform.position - gameObject.transform.position;
        toPlayer.y = 0;
        var fromPlayer = Quaternion.Euler(0, 90, 0) * toPlayer.normalized;
        var target = Player.transform.position + (fromPlayer * 15f);
        var toTarget = target - gameObject.transform.position;
        toTarget.y = 0;
        ChangeDirection(toTarget.normalized);
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
            Die();
        }
    }

    public void OnHealthPercentChanged(float value)
    {
        if (HealthBar != null)
        {
            HealthBar.HealthPercent = value;
        }
        else if (Pool != null && value > 0f)
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
        //Debug.Log(gameObject.name + " died.");

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
