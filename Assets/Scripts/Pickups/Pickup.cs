using UnityEngine;

public enum PickupType
{
    ShieldRestore,
    Gems,
    Upgrade
}

public class Pickup : MonoBehaviour
{
    public enum PickupState
    {
        Deploying,
        Ready,
        Collecting,
        Collected
    }

    [Header("In Game Values")]
    public PickupPoolSO Pool;
    public bool InUse;

    [Header("Definition Values")]
    public PickupValuesSO Values;
    public PickupType Type;
    public SphereCollider PickupCollider;
    public AudioSource Audio;
    public AudioClip PickupSound;
    public float MaxLifespan = 10f;

    [HideInInspector]
    public System.Action OnCollected;
    [HideInInspector]
    public System.Action OnExpired;

    [HideInInspector]
    public GameSceneToolsSO Tools;

    private PickupState State = PickupState.Ready;
    private GameObject CollectTarget;
    private float CollectDistance;
    private float Lifespan;
    private float BlinkValue;
    private Vector3 DeployVector;

    // Do anything necessary after values have been set.
    public virtual void Initialize(int idx)
    {
        var currentRotation = transform.rotation.eulerAngles;
        currentRotation.y = Random.Range(0f,360f);
        gameObject.transform.rotation = Quaternion.Euler(currentRotation);
        gameObject.transform.localScale = Vector3.one;
        CollectTarget = Tools.Player.gameObject;

        var HeadCollider = Tools.Player.PickupCollider;
        bool isOverlapping = Physics.ComputePenetration(
            PickupCollider, PickupCollider.transform.position, PickupCollider.transform.rotation,
            HeadCollider, HeadCollider.transform.position, HeadCollider.transform.rotation,
            out Vector3 direction, out float distance
        );
        if (isOverlapping)
        {
            StartCollecting();
        }
        else
        {
            Lifespan = MaxLifespan;
            State = PickupState.Deploying;
            var impulse = Random.insideUnitSphere.normalized * (0.1f * idx);
            DeployVector = new Vector3(impulse.x * Values.SpawnRadius, Mathf.Abs(impulse.y) * Values.SpawnHeight, impulse.z * Values.SpawnRadius);
            BlinkValue = 0f;
        }
    }

    public virtual void ReturnToPool()
    {
        Pool.ReturnPickup(this);
    }

    // Update is called once per frame
    void Update()
    {
        switch (State)
        {
            case PickupState.Deploying:
                var deployDelta = Time.deltaTime * Values.SpawnSpeed;
                var position = gameObject.transform.position + DeployVector * deployDelta;
                var y = Tools.Ter.SampleHeight(position);
                if (y > position.y)
                {
                    position.y = y;
                    State = PickupState.Ready;
                }
                gameObject.transform.position = position;
                DeployVector.y -= deployDelta;
                break;

            case PickupState.Ready:
                Lifespan -= Time.deltaTime;
                if (Lifespan <= 0f)
                {
                    OnExpired?.Invoke();
                    ReturnToPool();
                }
                if (Lifespan <= Values.DisappearWarningTime)
                {
                    BlinkValue += Time.deltaTime * Values.BlinkSpeed;
                    var size = 1f + Mathf.Sin(BlinkValue) * Values.BlinkScale;
                    gameObject.transform.localScale = new Vector3(size, size, size);
                    position = gameObject.transform.position;
                    position.y -= Values.SinkSpeed * Time.deltaTime;
                    gameObject.transform.position = position;
                }
                break;

            case PickupState.Collecting:
                CollectDistance -= Values.PickupVelocity * Time.deltaTime;
                if (CollectDistance <= Values.CollectRadius)
                {
                    State = PickupState.Collected;
                    Audio.PlayOneShot(PickupSound);
                    ReturnToPool();
                    OnCollected?.Invoke();
                    return;
                }

                var rotation = Values.PickupRotation * Time.deltaTime;
                var toPickup = gameObject.transform.position - CollectTarget.transform.position;
                if (toPickup.y > 0)
                    toPickup.y = Mathf.Max(0f, toPickup.y - Time.deltaTime);
                if (toPickup.y < 0)
                    toPickup.y = Mathf.Min(0f, toPickup.y + Time.deltaTime);
                toPickup = Quaternion.Euler(0f, rotation, 0f) * toPickup;
                toPickup = toPickup.normalized;
                toPickup *= CollectDistance;
                gameObject.transform.position = toPickup + CollectTarget.transform.position;
                break;
        }
    }

    public void StartCollecting()
    {
        State = PickupState.Collecting;
        var toPickup = gameObject.transform.position - CollectTarget.transform.position;
        CollectDistance = toPickup.magnitude;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (State >= PickupState.Collecting)
        {
            return;
        }

        if (other != Tools.Player.PickupCollider)
        {
            return;
        }
        StartCollecting();
    }
}
