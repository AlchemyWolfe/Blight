using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(menuName = "Blight/PickupPool", fileName = "SO_PickupPool_")]
public class PickupPoolSO : ScriptableObject
{
    [SerializeField]
    public Pickup PickupPrefab;

    [SerializeField, HideInInspector]
    public ObjectPool<Pickup> PickupPool;

    public void Initialize()
    {
        if (PickupPool == null)
        {
            PickupPool = new ObjectPool<Pickup>(OnCreatePickup, OnGetPickup, OnReleasePickup, OnDestroyPickup, false, 10, 100);
        }
        PickupPool.Clear();
    }

    public Pickup CreatePickup(Transform transform, int idx, GameSceneToolsSO tools, System.Action OnCollectedReceived, System.Action OnExpiredReceived)
    {
        var pickup = PickupPool.Get();
        pickup.gameObject.transform.position = transform.position;
        pickup.gameObject.transform.SetParent(transform.parent);
        pickup.Tools = tools;
        if (pickup.OnCollected == null)
        {
            pickup.OnCollected += OnCollectedReceived;
        }
        if (pickup.OnExpired == null)
        {
            pickup.OnExpired += OnExpiredReceived;
        }
        pickup.Initialize(idx);
        return pickup;
    }

    public void ReturnPickup(Pickup pickup)
    {
        if (pickup.InUse)
        {
            PickupPool.Release(pickup);
        }
    }

    private Pickup OnCreatePickup()
    {
        var pickup = GameObject.Instantiate(PickupPrefab);
        pickup.Pool = this;
        return pickup;
    }

    private void OnGetPickup(Pickup pickup)
    {
        pickup.gameObject.SetActive(true);
        pickup.InUse = true;
    }

    private void OnReleasePickup(Pickup pickup)
    {
        pickup.gameObject.SetActive(false);
        pickup.InUse = false;
    }

    private void OnDestroyPickup(Pickup pickup)
    {
        Destroy(pickup.gameObject);
    }
}
