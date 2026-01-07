using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnSprite : MonoBehaviour
{
    [SerializeField] private Animator _anim;

    [SerializeField] private SpriteRenderer _eyesSpriteRend;
    [SerializeField] private SpriteRenderer _browSpriteRend;
    [SerializeField] private SpriteRenderer _hairSpriteRend;
    [SerializeField] private SpriteRenderer _fhairSpriteRend;
    [SerializeField] private SpriteRenderer _headSpriteRend;
    [SerializeField] private SpriteRenderer _bodySpriteRend;
    [SerializeField] private SpriteRenderer _helmSpriteRend;
    [SerializeField] private SpriteRenderer _weaponSpriteRend;
    [SerializeField] private SpriteRenderer _offhandSpriteRend;

    [SerializeField] private Sprite _deadEyesSprite;

    [SerializeField] 
    private ParticleSystem _moveDust;

    private ArmorItemData _currentHelm;
    private ShieldItemData _currentShield;

    private FacingDirection _currentFacing = FacingDirection.West;
    private enum FacingDirection
    {
        West,
        East
    }

    private FacingDirection _lastFacing;

    #region UnityEvents

    private void Start()
    {
        _anim = GetComponent<Animator>();
    }

    #endregion

    public void SetData(Pawn pawn)
    {
        _currentHelm = pawn.GameChar.HelmItem;
        _currentShield = pawn.GameChar.ShieldItem;

        if (pawn.GameChar.TheWeapon != null)
        {
            _weaponSpriteRend.sprite = pawn.GameChar.TheWeapon.Data.itemSprite;
        }

        if (pawn.GameChar.HelmItem != null)
        {
            _helmSpriteRend.sprite = _currentHelm.SWSprite;
            _hairSpriteRend.gameObject.SetActive(false);
        }
        
        if (pawn.GameChar.ShieldItem != null)
        {
            _offhandSpriteRend.sprite = pawn.GameChar.ShieldItem.SWSprite;
        }

        
        if (pawn.GameChar.BodySprite != null)
        {
            _bodySpriteRend.sprite = pawn.GameChar.BodySprite;
        }

        _eyesSpriteRend.sprite = pawn.GameChar.SWEyesSprite;
        _hairSpriteRend.sprite = pawn.GameChar.HairDetail.SWSprite;
        _fhairSpriteRend.sprite = pawn.GameChar.FacialHairDetail.SWSprite;
        _browSpriteRend.sprite = pawn.GameChar.BrowDetail.SWSprite;
        _helmSpriteRend.sprite = _currentHelm?.SWSprite;
        _offhandSpriteRend.sprite = _currentShield?.SWSprite;


        _anim.Play("IdleSW", 0, Random.Range(0f, 1f));

        if (pawn.OnPlayerTeam)
        {
            UpdateFacingAndSpriteOrder(Vector3.zero, new Vector3(1, 0), pawn.CurrentTile);
        }
        else
        {
            UpdateFacingAndSpriteOrder(Vector3.zero, new Vector3(-1, 0), pawn.CurrentTile);
        }
    }

    private void FlipRotation()
    {
        transform.parent.Rotate(0, 180, 0, Space.Self);
    }

    public void HandleTurnEnd()
    {
        _anim.SetBool("MyTurn", false);
    }

    public void HandleTurnBegin()
    {
        _anim.SetBool("MyTurn", true);
        _anim.Play("ActivatedSW");
    }

    /// <summary>
    /// Update based on last position / new position
    /// </summary>
    /// <param name="originPosition"></param>
    /// <param name="facingPosition"></param>
    /// <remarks>
    /// Perhaps worth having the currentPosition, even though the sprite
    /// component is child of the pawn anyway - because the sprite might be moving
    /// as a result from animation.
    /// </remarks>
    public void UpdateFacingAndSpriteOrder(Vector3 originPosition, Vector3 facingPosition, Tile currentTile = null)
    {
        int totalSpriteOrder = 0;
        if (currentTile != null)
        {
            // make sure he's rendered in front of and behind appropriate terrain pieces
            totalSpriteOrder = -(int)(currentTile.transform.position.y * GridGenerator.TILE_SPRITE_LAYER_SEPARATION_MULTIPLIER);
        }

        _bodySpriteRend.sortingOrder = totalSpriteOrder;

        _eyesSpriteRend.sortingOrder = totalSpriteOrder + 2;
        _browSpriteRend.sortingOrder = totalSpriteOrder + 2;
        // same level as helm
        _hairSpriteRend.sortingOrder = totalSpriteOrder + 4;
        _fhairSpriteRend.sortingOrder = totalSpriteOrder + 2;
        _headSpriteRend.sortingOrder = totalSpriteOrder + 1;

        _helmSpriteRend.sortingOrder = totalSpriteOrder + 4;

        // going NE
        if (facingPosition.x > originPosition.x)
        {
            UpdateSpriteFacing(FacingDirection.East);
        }
        // going NW
        else if (facingPosition.x < originPosition.x)
        {
            UpdateSpriteFacing(FacingDirection.West);
        }
    }

    private void UpdateSpriteFacing(FacingDirection newDirection)
    {
        _lastFacing = _currentFacing;
        _currentFacing = newDirection;

        HandleRotation();
    }

    #region Animations

    public void PlayAttack(Vector3 attackDirection)
    {
        StartCoroutine(AttackAnimation(attackDirection));
    }

    // since this is direction dependent, it's done in code.
    private IEnumerator AttackAnimation(Vector3 direction)
    {
        Transform t = transform.parent;

        Vector3 startPos = t.position;

        float lungeDistance = 0.5f;
        Vector3 lungePos = startPos + direction * lungeDistance;

        float forwardTime = 0.05f;
        float returnTime = 0.35f;

        // Fast lunge
        float t01 = 0f;
        while (t01 < 1f)
        {
            t01 += Time.deltaTime / forwardTime;
            t.position = Vector3.Lerp(startPos, lungePos, t01);
            yield return null;
        }

        // Slower return
        t01 = 0f;
        while (t01 < 1f)
        {
            t01 += Time.deltaTime / returnTime;
            t.position = Vector3.Lerp(lungePos, startPos, t01);
            yield return null;
        }

        t.position = startPos; // snap for safety
    }

    public void Move()
    {
        _anim.Play("WalkSW");
    }

    public void HandleRotation()
    {
        switch (_currentFacing)
        {
            case FacingDirection.East:
                if (_lastFacing == FacingDirection.West)
                {
                    FlipRotation();
                }
                break;

            case FacingDirection.West:
                if (_lastFacing == FacingDirection.East)
                {
                    FlipRotation();
                }
                break;
        }
    }

    public void StopMoving()
    {    
        _anim.Play("IdleSW", 0, Random.Range(0f, 1f));
    }

    public void Die()
    {
        StartCoroutine(PlayAnimationAfterDelay(.2f, "Die"));
        
        _eyesSpriteRend.sprite = _deadEyesSprite;

        _hairSpriteRend.sortingLayerName = "DeadCharacters";
        _browSpriteRend.sortingLayerName = "DeadCharacters";
        _fhairSpriteRend.sortingLayerName = "DeadCharacters";
        _eyesSpriteRend.sortingLayerName = "DeadCharacters";
        _bodySpriteRend.sortingLayerName = "DeadCharacters";
        _headSpriteRend.sortingLayerName = "DeadCharacters";
        _helmSpriteRend.sortingLayerName = "DeadCharacters";
        _weaponSpriteRend.sortingLayerName = "DeadCharacters";
        _offhandSpriteRend.sortingLayerName = "DeadCharacters";
    }

    public void HandleHit(bool isDead, bool armorHit, bool armorDestroyed)
    {
        string animationString = "";
        if (armorHit)
        {
            StartCoroutine(PlayArmorHitFXAfterDelay(0f));
            animationString = "GetHitARMRSW";
        }
        else
        {
            StartCoroutine(PlayBloodSpurtAfterDelay(0f));
            animationString = "GetHitHPSW";
        }

        if (!isDead)
        {
            _anim.Play(animationString);
        }

        // make the helmet gone if there's no armor for cool factor
        if (armorDestroyed)
        {
            _helmSpriteRend.enabled = false;
        }
    }

    public void OnLevelUpAnimComplete()
    {
        _anim.SetBool("LevelUp", false);
    }

    public void SetLevelUp()
    {
        _anim.SetBool("LevelUp", true);
    }

    public void TriggerBlock()
    {
        StartCoroutine(PlayAnimationAfterDelay(.2f, "Block"));
    }

    public void TriggerDodge()
    {
        StartCoroutine(PlayAnimationAfterDelay(.2f, "DodgeSW"));
    }

    #endregion

    #region FX

    private IEnumerator PlayArmorHitFXAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        BattleManager.Instance.PlayArmorHitFX(transform.position + (Vector3.up * .3f));
    }

    private IEnumerator PlayBloodSpurtAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        BattleManager.Instance.PlayBloodSpurt(transform.position + (Vector3.up * .3f));
    }

    private IEnumerator PlayAnimationAfterDelay(float delay, string animName)
    {
        yield return new WaitForSeconds(delay);

        _anim.Play(animName);
    }

    #endregion

}
