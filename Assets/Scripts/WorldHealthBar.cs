using UnityEngine;
using UnityEngine.UI;

public class WorldHealthBar : MonoBehaviour
{
    [SerializeField]
    public Slider HealthSlider;

    //[HideInInspector]
    public Canvas WorldCanvas;

    //[HideInInspector]
    public GameObject FollowTarget;

    [HideInInspector]
    public HealthBarPoolSO Pool;

    [HideInInspector]
    public bool InUse;

    void Update()
    {
        transform.position = FollowTarget.transform.position;
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
