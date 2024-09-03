using System.Collections.Generic;
using UnityEngine;

public class TargetIndicator : MonoBehaviour
{
    [Header("In Game Values")]
    public TargetIndicatorSO Pool;
    public bool InUse;

    [Header("Definition Values")]
    public float Orbit = 2;
    public float MinRange = 5;
    public List<GameObject> Indicators;

    private bool TargetSet = false;
    private GameObject Target;
    private Player PlayerWolf;
    private IndicatorIcon Icon;

    public enum IndicatorIcon
    {
        Enemy,
        Boss,
        PowerUp,
        Gems,
        Shield
    }

    public void SetTarget(GameObject target, Player player, IndicatorIcon icon)
    {
        Target = target;
        PlayerWolf = player;
        TargetSet = true;
        Icon = icon;
        for (int i = 0; i < Indicators.Count; i++)
        {
            var indicator = Indicators[i];
            if (indicator == null)
            {
                continue;
            }
            int idx = (int)icon;
            indicator.SetActive(i == idx);
        }
    }

    void Update()
    {
        if (TargetSet == false)
        {
            return;
        }
        if (!Target.activeInHierarchy || PlayerWolf.IsDying)
        {
            Pool.ReturnIndicator(this);
        }
        var direction = Target.transform.position - PlayerWolf.transform.position;
        var position = PlayerWolf.transform.position + (direction.normalized * Orbit);
        int idx = (int)Icon;
        var indicator = Indicators[idx];
        if (direction.magnitude < MinRange)
        {
            indicator.SetActive(false);
        }
        else
        {
            indicator.SetActive(true);
            transform.position = position;
            transform.rotation = Target.transform.rotation;
        }
    }
}
