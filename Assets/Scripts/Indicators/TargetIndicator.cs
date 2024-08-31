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
    private GameObject Player;
    private IndicatorIcon Icon;

    public enum IndicatorIcon
    {
        Enemy,
        Boss,
        PowerUp,
        Gems,
        Shield
    }

    public void SetTarget(GameObject target, GameObject player, IndicatorIcon icon)
    {
        Target = target;
        Player = player;
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
        if (Target == null || Player == null)
        {
            Pool.ReturnIndicator(this);
        }
        var direction = Target.transform.position - Player.transform.position;
        var position = Player.transform.position + (direction.normalized * Orbit);
        if (direction.magnitude < MinRange)
        {
            position.y -= 50f;
        }
        transform.position = position;
        transform.rotation = Target.transform.rotation;
    }
}
