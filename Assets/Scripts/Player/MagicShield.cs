using MalbersAnimations;
using System.Collections.Generic;
using UnityEngine;

public class MagicShield : MonoBehaviour
{
    // 20 is Player, 21 is Destructible, and 23 is Enemy.
    public static HashSet<int> ValidLayers = new() { 20, 23 };

    public MagicMaterialsSO MagicMaterials;

    public GameSceneToolsSO Tools;
    public MeshRenderer Renderer;
    public BlightCreature Wielder;

    public GameObject SphereColor;
    public GameObject SphereOutline;

    public StatID Stat;
    public float Damage;
    public AudioSource Audio;
    public AudioClip ShieldDownClip;
    public AudioClip ShieldRestoredClip;

    private bool IsActive;

    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        gameObject.layer = Wielder.gameObject.layer;
        var magicMaterial = Wielder.Magic == null ? null : Wielder.Magic.material;
        Renderer.sharedMaterial = MagicMaterials.GetMatchingWindMaterial(magicMaterial);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsActive)
        {
            return;
        }

        var otherLayer = other.gameObject.layer;
        if (otherLayer == gameObject.layer || !ValidLayers.Contains(otherLayer))
        {
            return;
        }

        // We've hit something not aligned with us, let's see if we can damage it!
        var otherRigidBody = other.attachedRigidbody;
        if (otherRigidBody == null)
        {
            return;
        }
        if (otherRigidBody.TryGetComponent<BlightCreature>(out var blightCreature))
        {
            if (blightCreature.IsDying)
            {
                return;
            }
        }
        var baseObject = otherRigidBody.gameObject;
        if (baseObject.TryGetComponent<IMDamage>(out var damagable))
        {
            var modifier = new StatModifier() { ID = Stat, modify = StatOption.SubstractValue, Value = Damage };
            damagable.ReceiveDamage(-transform.forward,
                Wielder.gameObject,
                modifier,
                false,
                true,
                null,
                false,
                null);
        }
        // Either way, we're down.
        DeactivateShield();
    }

    public void ActivateShield(bool silent = false)
    {
        if (Wielder.IsDying)
        {
            return;
        }
        IsActive = true;
        if (!silent)
            Audio.PlayOneShot(ShieldRestoredClip);
        SphereColor.SetActive(true);
        SphereOutline.SetActive(true);
    }

    public void DeactivateShield(bool silent = false)
    {
        IsActive = false;
        if (!silent)
            Audio.PlayOneShot(ShieldDownClip);
        SphereColor.SetActive(false);
        SphereOutline.SetActive(false);
        Tools.OnShieldDown?.Invoke();
    }
}
