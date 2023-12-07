using System;
using UnityEngine;

public class DestructibleDecoration : MonoBehaviour
{
    public float CoinPerHPValue = 0.01f;
    public float EnergyPerHPValue = 0.01f;
    public float HealingPerHPValue = 0.01f;
    public HealthBarPoolSO HealthBarPool;
    public WorldHealthBar HealthBar;

    [HideInInspector]
    public Action<DestructibleDecoration> OnKilled;
    [HideInInspector]
    public Decoration decoration;

    private bool IsDying;

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
            HealthBar.HealthSlider.value = value;
        }
        else
        {
            HealthBar = HealthBarPool.CreateHealthBar(gameObject);
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

        Destroy(gameObject);
    }
}
