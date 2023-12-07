using MalbersAnimations;
using UnityEngine;

public class MoveTowardsMouse : MonoBehaviour
{
    public LayerMask groundLayer;
    private Vector3 moveDirection;
    private float EPSIOLON = 0.001f;
    private ICharacterMove CharacterMove;

    private void Start()
    {
        CharacterMove = GetComponent<ICharacterMove>();
    }

    void Update()
    {
        // Cast a ray from the mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Check if the ray hits something on the ground layer
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            var newDirection = hit.point - transform.position;
            newDirection.y = 0f;
            if (newDirection.sqrMagnitude > EPSIOLON)
            {
                moveDirection = newDirection.normalized;
            }
        }
        CharacterMove.SetInputAxis(moveDirection);
    }
}
