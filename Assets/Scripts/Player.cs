using DG.Tweening;
using MalbersAnimations;
using UnityEngine;

public class Player : BlightCreature
{
    public GameSceneToolsSO Tools;
    public Collider PickupCollider;
    public ExplosionPoolSO Barksplosion;
    public AudioSource PantAudio;
    public AudioClip PantingSound;
    public LayerMask GroundLayer;
    public bool FreezeMovement;

    [HideInInspector]
    public bool IsInjured;

    private Vector3 moveDirection;
    private static readonly float EPSIOLON = 0.001f;
    private ICharacterMove CharacterMove;

    private void Start()
    {
        CharacterMove = GetComponent<ICharacterMove>();
        Tools.Player = this;
    }

    public void StartGame()
    {
        PantAudio.clip = PantingSound;
        PantAudio.loop = true;
        PantAudio.pitch = 0.67f;
        PantAudio.Play();
        InitializeWeapons();
        DOVirtual.DelayedCall(1f, StartAttacking);
    }

    private void PauseGame(bool paused)
    {
        if (paused)
        {
            PantAudio.Play();
        }
        else
        {
            PantAudio.Pause();
        }
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

    public void Bark()
    {
        var explosion = Barksplosion.CreateExplosion(gameObject, transform.position, 1, Magic.material);
        explosion.StartExploding();
    }
}
