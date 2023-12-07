using DG.Tweening;
using MalbersAnimations;
using UnityEngine;

public class Enemy : BlightCreature
{
    public SkinnedMeshRenderer Skin;
    public SkinnedMeshRenderer Magic;

    [HideInInspector]
    public EnemyDefinitionSO Pool;

    [HideInInspector]
    public bool InUse;

    private Vector3 InputDurection;
    private ICharacterMove CharacterMove;

    private void Start()
    {
        CharacterMove = GetComponent<ICharacterMove>();
        CharacterMove.SetInputAxis(InputDurection);
    }

    public void Reset()
    {
        Debug.Log("We still need to reset enemies health.");
        DOVirtual.DelayedCall(1f, StartAttacking);
    }

    public void SetSkin(Material material)
    {
        if (Skin == null)
        {
            Skin = GetComponent<SkinnedMeshRenderer>();
        }
        if (Skin != null)
        {
            Skin.material = material;
        }
    }

    public void SetMagic(bool isMagic)
    {
        Magic.enabled = isMagic;
    }

    public void ChangeDirection(Vector3 newDirection)
    {
        InputDurection = newDirection;
        if (CharacterMove != null)
        {
            CharacterMove.SetInputAxis(InputDurection);
        }
    }

    void Update()
    {
    }
}
