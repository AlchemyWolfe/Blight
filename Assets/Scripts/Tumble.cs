using UnityEngine;

public class Tumble : MonoBehaviour
{
    public GameObject MainGO;
    public float TumbleSpeed = 1f;

    private Vector3 RandomAxis;

    // Start is called before the first frame update
    void Start()
    {
        RandomAxis = Random.onUnitSphere * TumbleSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        MainGO.transform.Rotate(RandomAxis, TumbleSpeed * Time.deltaTime);
    }
}
