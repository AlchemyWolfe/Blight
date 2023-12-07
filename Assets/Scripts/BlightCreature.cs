using MalbersAnimations.Controller;
using System.Collections.Generic;
using UnityEngine;

public class BlightCreature : MonoBehaviour
{
    [Tooltip("The Animal component.")]
    public MAnimal Weilder;

    [Tooltip("The point to fire from, plus the projectile's initial orientation.")]
    public GameObject Muzzle;

    [Tooltip("This should be set to Internal Components, CameraTarget.")]
    public GameObject Center;

    [Tooltip("Definitions for all of the guns this weilder has.")]
    public List<AutoAttackSO> AttakDefinitions;

    private GameObject ProjectileContainer;
    private List<AutoAttack> Attacks;

    public void StartAttacking()
    {
        ProjectileContainer = Weilder.gameObject.transform.parent.gameObject;
        if (Attacks == null)
        {
            Attacks = new List<AutoAttack>();
        }
        Attacks.Clear();
        foreach (AutoAttackSO attackDefinition in AttakDefinitions)
        {
            StartAttack(attackDefinition);
        }
    }

    public void StartAttack(AutoAttackSO attackDefinition,
        int rateOfFireLevel = 0,
        int followShotLevel = 0,
        int parallelLevel = 0,
        int damageLevel = 0,
        int velocityLevel = 0,
        int sizeLevel = 0)
    {
        var attack = new AutoAttack(attackDefinition, Weilder, Muzzle, ProjectileContainer,
            rateOfFireLevel, followShotLevel, parallelLevel,
            damageLevel, velocityLevel, sizeLevel);
        attack.StartAttacking();
        Attacks.Add(attack);
    }

    public void StopAttacking()
    {
        if (Attacks == null)
        {
            return;
        }
        foreach (AutoAttack attack in Attacks)
        {
            attack.StopAttacking();
        }
        Attacks.Clear();
    }
}
