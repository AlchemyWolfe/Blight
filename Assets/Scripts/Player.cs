using DG.Tweening;
using MalbersAnimations;
using UnityEngine;

public class Player : BlightCreature
{
    public LayerMask GroundLayer;
    public bool FreezeMovement;

    private Vector3 moveDirection;
    private static readonly float EPSIOLON = 0.001f;
    private ICharacterMove CharacterMove;

    private void Start()
    {
        CharacterMove = GetComponent<ICharacterMove>();
        DOVirtual.DelayedCall(1f, StartAttacking);
    }

    void Update()
    {
        if (FreezeMovement)
        {
            return;
        }

        // Cast a ray from the mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Check if the ray hits something on the ground layer
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, GroundLayer))
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
