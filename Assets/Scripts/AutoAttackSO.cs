using UnityEngine;

[CreateAssetMenu(menuName = "Blight/AutoAttackDefinition", fileName = "SO_AutoAttack_")]
public class AutoAttackSO : ScriptableObject
{
    [Tooltip("Type of projectile fired")]
    public ProjectilePoolSO ProjectileDefinition;

    [Tooltip("Seconds between bursts.  Should be multiples of 0.02f.")]
    public FloatPerLevel RateOfFire;

    [Tooltip("If false, we wait for the next ROF.")]
    public bool FireImmediately;

    [Tooltip("Seconds between projectiles created per burst.  Should be multiples of 0.02f.")]
    public float FollowupShotSpeed = 0.04f;

    [Tooltip("Sound to play when the attack fires.")]
    public AudioClip FireSound;

    [Tooltip("Sound to play during followup projectiles are created in a burst.")]
    public AudioClip ExtraBurstSound;

    [Tooltip("The number of projectiles to fire after the original.")]
    public IntPerLevel FollowShotCount;

    [Tooltip("The number of projectiles to fire at once, spaced evenly.")]
    public IntPerLevel ParallelShots;

    [Tooltip("Distance between parallel shots, scales with projectile size level.")]
    public FloatPerLevel ParallelShotSpacing;

    [Tooltip("Multiplier on damage and damage increases.")]
    public float DamageMultiplier = 1f;

    [Tooltip("Multiplier on velocity and velocity increases.")]
    public float VelocityMultiplier = 1f;

    [Tooltip("Multiplier on size and size increases.")]
    public float SizeMultiplier = 1f;

    [Tooltip("Multiplier on the distance the projectiles can go.")]
    public float LifespanMultiplier = 1f;

    private void OnValidate()
    {
        RateOfFire.SetMinMax(0.02f, 60f);
        ParallelShotSpacing.SetMinMax(0f, 10f);
        ParallelShots.SetMinMax(1, 20);
        if (LifespanMultiplier <= 0f)
        {
            LifespanMultiplier = 1f;
        }
    }
}
