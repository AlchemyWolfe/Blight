using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class WorldHealthBar : MonoBehaviour
{
    public Slider HealthSlider;

    [PropertyRange(0f, 1f)]
    public float _healthPercent;
    public float HealthPercent
    {
        get => _healthPercent;
        set
        {
            _healthPercent = value;
            SetBarValue();
        }
    }

    //[HideInInspector]
    public Canvas WorldCanvas;

    //[HideInInspector]
    public BlightCreature CreatureTarget;
    public GameObject OtherTarget;

    [HideInInspector]
    public WorldHealthBarDefinitionSO Pool;

    [HideInInspector]
    public bool InUse;

    private void OnValidate()
    {
        //SetBarValue();
    }

    void Update()
    {
        var followPosition = CreatureTarget != null ? CreatureTarget.Center.transform.position : OtherTarget.transform.position;
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
        transform.SetParent(WorldCanvas.transform);
        if (target.TryGetComponent<BlightCreature>(out var creature))
        {
            CreatureTarget = creature;
        }
        else
        {
            OtherTarget = target;
        }
    }

    private void SetBarValue()
    {
        if (HealthPercent <= 0f)
        {
            ReturnToPool();
            return;
        }

        HealthSlider.gameObject.SetActive(HealthPercent < 1f);
        HealthSlider.value = HealthPercent;
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
