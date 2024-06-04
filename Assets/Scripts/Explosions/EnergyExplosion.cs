using MalbersAnimations;
using System.Collections.Generic;
using UnityEngine;

public class EnergyExplosion : MonoBehaviour
{
    // 20 is Player, 21 is Destructible, and 23 is Enemy.
    public static HashSet<int> ValidLayers = new() { 20, 21, 23 };

    [Header("In Game Values")]
    public GameObject Attacker;
    public ExplosionPoolSO Pool;
    public bool InUse;
    public float Damage;
    public Material WindMaterial;

    [Header("Definition Values")]
    public GameObject Sphere;
    public GameObject Collider;
    public MeshRenderer Renderer;
    public float StartSize;
    public float EndSize;
    public float EndSizeY;
    public float Duration;
    public float Remain;

    private float ElapsedTime;
    private bool Exploding;
    private int _level;
    public int Level
    {
        get => _level;
        set
        {
            _level = value;
            SetLevelValues();
        }
    }

    // Adjust stats based on explosion level.
    public virtual void SetLevelValues()
    {
        Damage = 5f;
    }

    // Do anything necessary after values have been set.
    public virtual void Initialize()
    {
        if (WindMaterial != null)
        {
            Renderer.sharedMaterial = WindMaterial;
        }
    }

    public void StartExploding()
    {
        ElapsedTime = 0f;
        Sphere.SetActive(true);
        Sphere.transform.localScale = new Vector3(StartSize, StartSize, StartSize);
        Collider.SetActive(true);
        Collider.transform.localScale = new Vector3(EndSize, EndSizeY, EndSize);
        Exploding = true;
    }

    public void Reset()
    {
        Exploding = false;
        ElapsedTime = 0f;
        Sphere.SetActive(false);
    }

    private void Update()
    {
        if (!Exploding)
        {
            return;
        }

        ElapsedTime += Time.deltaTime;
        var progress = Mathf.Min(1.0f, ElapsedTime / Duration);
        var size = Mathf.Lerp(StartSize, EndSize, progress);
        var sizeY = Mathf.Lerp(StartSize, EndSizeY, progress);
        Sphere.transform.localScale = new Vector3(size, sizeY, size);

        if (ElapsedTime >= Duration + Remain)
        {
            Fizzle();
        }
    }

    // Do anything that happens when we reach the end of our lifespan.
    public virtual void Fizzle()
    {
        ReturnToPool();
    }

    public void ReturnToPool()
    {
        Pool.ReturnExplosion(this);
    }

    private void OnTriggerEnter(Collider other)
    {
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
        var baseObject = otherRigidBody.gameObject;
        if (baseObject.TryGetComponent<IMDamage>(out var damagable))
        {
            var modifier = new StatModifier() { ID = Pool.Stat, modify = StatOption.SubstractValue, Value = 1f/*Damage*/ };
            damagable.ReceiveDamage(-transform.forward,
                Attacker,
                modifier,
                false,
                true,
                null,
                false,
                null);
        }
    }
}
