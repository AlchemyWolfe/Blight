using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectFollow : MonoBehaviour
{
    public Transform Target;

    private Vector3 InitialOffset;

    void Awake()
    {
        InitialOffset = transform.position - Target.position;
    }

    void Update()
    {
        transform.position = InitialOffset + Target.position;
    }
}
