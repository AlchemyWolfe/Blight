using DG.Tweening;
using MalbersAnimations;
using System;
using UnityEngine;

public class Player : BlightCreature
{
    public PlayerDataSO PlayerData;
    public Collider PickupCollider;
    public ExplosionPoolSO Barksplosion;
    public AudioSource PantAudio;
    public AudioClip PantingSound;
    public AudioClip HurtSound;
    public AudioClip DieSound;
    public LayerMask GroundLayer;
    public bool FreezeMovement;
    public bool _freezeWeapons;
    public bool FreezeWeapons
    {
        get => _freezeWeapons;
        set
        {
            _freezeWeapons = value;
            if (value)
            {
                StopAttacking();
            }
            else
            {
                StartAttacking();
            }
        }
    }

    [HideInInspector]
    public Action OnKilled;

    private Vector3 moveDirection;
    private static readonly float EPSIOLON = 0.001f;
    private ICharacterMove CharacterMove;
    private float PrevHealth;

    private void Awake()
    {
        Tools.Player = this;
        AddListeners();
        IsDying = true;
        SetSkinColor(PlayerData.ChosenSkin);
        SetMagicColor(PlayerData.ChosenMagic);
        SetPositionOnGround();
    }

    public void AddListeners()
    {
        PlayerData.OnSkinChoiceChanged += OnSkinChoiceChangeReceived;
        PlayerData.OnMagicChoiceChanged += OnMagicChoiceChangeReceived;
        Tools.OnTerrainInitialized += OnTerrainInitializedReceived;
        Tools.OnGameStart += OnGameStartReceived;
        Tools.OnGameOver += OnGameOverReceived;
        Tools.OnGameClose += OnGameCloseReceived;
    }

    private void OnDestroy()
    {
        RemoveListeners();
    }

    public void RemoveListeners()
    {
        PlayerData.OnSkinChoiceChanged -= OnSkinChoiceChangeReceived;
        PlayerData.OnMagicChoiceChanged -= OnMagicChoiceChangeReceived;
        Tools.OnTerrainInitialized -= OnTerrainInitializedReceived;
        Tools.OnGameStart -= OnGameStartReceived;
        Tools.OnGameOver -= OnGameOverReceived;
        Tools.OnGameClose -= OnGameCloseReceived;
    }

    private void Start()
    {
        CharacterMove = GetComponent<ICharacterMove>();
        PantAudio.clip = PantingSound;
        PantAudio.loop = true;
        PantAudio.pitch = 0.67f;
        PantAudio.Stop();
    }

    public void OnTerrainInitializedReceived()
    {
        SetPositionOnGround();
    }

    public void OnSkinChoiceChangeReceived()
    {
        SetSkinColor(PlayerData.ChosenSkin);
    }

    public void OnMagicChoiceChangeReceived()
    {
        SetMagicColor(PlayerData.ChosenMagic);
    }

    public void OnGameStartReceived()
    {
        gameObject.layer = 20;  // Player
        IsDying = false;
        PantAudio.Play();
        Shield.DeactivateShield(true);
        InitializeWeapons();
        StopAttacking();
        if (!FreezeWeapons)
        {
            DOVirtual.DelayedCall(1f, ReallyStartAttaking);
        }
    }

    public void ReallyStartAttaking()
    {
        StartAttacking();
    }

    public void OnGameOverReceived()
    {

    }

    public void OnGameCloseReceived()
    {
        Shield.DeactivateShield(true);
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
        if (FreezeMovement || IsDying)
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

    public void OnHealthChanged(float value)
    {
        //Debug.Log(gameObject.name + " is at " + value + "HP.");
    }

    public void OnHealthPercentChanged(float value)
    {
        //Debug.Log(gameObject.name + " is at " + value + "percent.");
        if (value < PrevHealth && Wielder.ActiveState.ID != StateEnum.Death)
        {
            if (value <= 0.01f)
            {
                Die();
            }
            else
            {
                PantAudio.PlayOneShot(HurtSound);
            }
        }
        PrevHealth = value;
    }

    public void Die()
    {
        if (IsDying)
        {
            return;
        }
        IsDying = true;
        gameObject.layer = 19;  // Dead
        StopAttacking();
        PantAudio.Stop();
        PantAudio.PlayOneShot(DieSound);
        OnKilled?.Invoke();
    }

    public void Bark()
    {
        if (IsDying)
        {
            return;
        }
        var magicMaterial = Magic == null ? null : Magic.material;
        var explosion = Barksplosion.CreateExplosion(gameObject, transform.position, 1, magicMaterial);
        explosion.StartExploding();
    }
}
