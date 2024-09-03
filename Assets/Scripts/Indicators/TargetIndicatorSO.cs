using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(menuName = "Blight/TargetIndicatorPool", fileName = "SO_TargetIndicatorPool")]
public class TargetIndicatorSO : ScriptableObject
{
    [SerializeField]
    public TargetIndicator IndicatorPrefab;

    [SerializeField, HideInInspector]
    public ObjectPool<TargetIndicator> IndicatorPool;

    public void Initialize()
    {
        if (IndicatorPool == null)
        {
            IndicatorPool = new ObjectPool<TargetIndicator>(OnCreateIndicator, OnGetIndicator, OnReleaseIndicator, OnDestroyIndicator, false, 10, 100);
        }
        IndicatorPool.Clear();
    }

    public TargetIndicator CreateIndicator(GameObject target, Player player, TargetIndicator.IndicatorIcon icon)
    {
        var indicator = IndicatorPool.Get();
        indicator.SetTarget(target, player, icon);
        return indicator;
    }

    public void ReturnIndicator(TargetIndicator indicator)
    {
        if (indicator.InUse)
        {
            IndicatorPool.Release(indicator);
        }
    }

    private TargetIndicator OnCreateIndicator()
    {
        var indicator = GameObject.Instantiate(IndicatorPrefab);
        indicator.Pool = this;
        return indicator;
    }

    private void OnGetIndicator(TargetIndicator indicator)
    {
        indicator.gameObject.SetActive(true);
        indicator.InUse = true;
    }

    private void OnReleaseIndicator(TargetIndicator indicator)
    {
        indicator.gameObject.SetActive(false);
        indicator.InUse = false;
    }

    private void OnDestroyIndicator(TargetIndicator indicator)
    {
        Destroy(indicator.gameObject);
    }
}
