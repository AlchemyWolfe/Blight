using MalbersAnimations;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class WorldHealthBar : MonoBehaviour
{
    [SerializeField]
    public Image HealthSplat;

    public Color FullHealthColor = new Color(4f / 256f, 190f / 255f, 4f / 256f, 0f);
    public Color ZeroHealthColor = new Color(190f / 256f, 4 / 256f, 4 / 256f, 1f);

    [PropertyRange(0f, 1f)]
    public float _healthPercent;
    public float HealthPercent
    {
        get => _healthPercent;
        set
        {
            _healthPercent = value;
            SetSplatColor();
        }
    }

    //[HideInInspector]
    public Canvas WorldCanvas;

    //[HideInInspector]
    public GameObject Target;
    public GameObject FollowTarget;

    [HideInInspector]
    public WorldHealthBarDefinitionSO Pool;

    [HideInInspector]
    public bool InUse;

    private void OnValidate()
    {
        SetSplatColor();
    }

    void Update()
    {
        var followPosition = FollowTarget.transform.position;
        followPosition.y += 1f;
        transform.position = followPosition;
    }

    public void Reset()
    {
        HealthPercent = 1f;
    }

    public void SetFollowTarget(GameObject target, Canvas worldCanvas)
    {
        WorldCanvas = worldCanvas;
        Target = target;
        if (Target.TryGetComponent<BlightCreature>(out var creature))
        {
            FollowTarget = creature.Center;
        }
        else
        {
            FollowTarget = Target;
        }

        var stats = Target.GetComponent<Stats>();
    }

    private void SetSplatColor()
    {
        var r = ZeroHealthColor.r + ((FullHealthColor.r - ZeroHealthColor.r) * HealthPercent);
        var g = ZeroHealthColor.g + ((FullHealthColor.g - ZeroHealthColor.g) * HealthPercent);
        var b = ZeroHealthColor.b + ((FullHealthColor.b - ZeroHealthColor.b) * HealthPercent);
        var a = ZeroHealthColor.a + ((FullHealthColor.a - ZeroHealthColor.a) * HealthPercent);
        HealthSplat.color = new Color(r, g, b, a);
    }

    public void ReturnToPool()
    {
        if (Pool != null)
        {
            Pool.ReturnHealthBar(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
