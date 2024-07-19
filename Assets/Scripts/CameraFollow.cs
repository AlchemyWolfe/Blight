using UnityEngine;

public class CameraFollow : MonoBehaviour

{
    public GameSceneToolsSO Tools;
    public Transform target;
    public Vector3 menuLocationOffset;
    public Vector3 menuRotationOffset;
    public Vector3 gameLocationOffset;
    public Vector3 gameRotationOffset;
    public bool JumpToPosition;

    private Vector3 locationOffset;
    private Vector3 rotationOffset;
    private Vector3 lastDelta;

    private void Awake()
    {
        Tools.OnGameStart += OnGameStartReceived;
        Tools.OnGameOver += OnGameOverReceived;
        Tools.OnGameClose += OnGameCloseReceived;
        locationOffset = menuLocationOffset;
        rotationOffset = menuRotationOffset;
    }

    public void OnGameStartReceived()
    {
        locationOffset = gameLocationOffset;
        rotationOffset = gameRotationOffset;
        target = Tools.Player.gameObject.transform;
    }

    public void OnGameOverReceived()
    {
        locationOffset = menuLocationOffset;
        rotationOffset = menuRotationOffset;
    }

    public void OnGameCloseReceived()
    {
        locationOffset = menuLocationOffset;
        rotationOffset = menuRotationOffset;
    }

    void Update()
    {
        if (JumpToPosition)
        {
            transform.position = target.position + locationOffset;
        }
        else
        {
            Vector3 desiredPosition = target.position + locationOffset;
            var delta = transform.position - desiredPosition;
            if (Vector3.Dot(lastDelta, delta) < 0)
            {
                JumpToPosition = true;
            }
            else
            {
                Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime);
                transform.position = smoothedPosition;
            }
            lastDelta = delta;
        }

        Quaternion desiredrotation = Quaternion.Euler(rotationOffset);
        Quaternion smoothedrotation = Quaternion.Lerp(transform.rotation, desiredrotation, Time.deltaTime);
        transform.rotation = smoothedrotation;
    }
}