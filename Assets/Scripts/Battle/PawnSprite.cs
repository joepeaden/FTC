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
    [SerializeField] private SpriteRenderer _shieldSpriteRend;

    [SerializeField] private Sprite _blankSprite;

    private Sprite _SWFace;
    private Sprite _SEFace;

    [SerializeField] private Sprite _SWDeadFace;
    [SerializeField] private Sprite _SEDeadFace;

    private Sprite _NEHair;
    private Sprite _NWHair;
    private Sprite _SWHair;
    private Sprite _SEHair;

    private Sprite _NEBrow;
    private Sprite _NWBrow;
    private Sprite _SWBrow;
    private Sprite _SEBrow;

    private Sprite _NEFacialHair;
    private Sprite _NWFacialHair;
    private Sprite _SWFacialHair;
    private Sprite _SEFacialHair;

    [SerializeField] 
    private ParticleSystem _moveDust;

    private ArmorItemData _currentHelm;
    private ShieldItemData _currentShield;

    private FacingDirection _facingDirection;
    private enum FacingDirection
    {
        NW,
        NE,
        SE,
        SW
    }

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
            _shieldSpriteRend.sprite = pawn.GameChar.ShieldItem.SWSprite;
        }

        _NEHair = pawn.GameChar.HairDetail.NESprite;
        _NWHair = pawn.GameChar.HairDetail.NWSprite;
        _SWHair = pawn.GameChar.HairDetail.SWSprite;
        _SEHair = pawn.GameChar.HairDetail.SESprite;

        _NEBrow = pawn.GameChar.BrowDetail.NESprite;
        _NWBrow = pawn.GameChar.BrowDetail.NWSprite;
        _SWBrow = pawn.GameChar.BrowDetail.SWSprite;
        _SEBrow = pawn.GameChar.BrowDetail.SESprite;

        _NEFacialHair = pawn.GameChar.FacialHairDetail.NESprite;
        _NWFacialHair = pawn.GameChar.FacialHairDetail.NWSprite;
        _SWFacialHair = pawn.GameChar.FacialHairDetail.SWSprite;
        _SEFacialHair = pawn.GameChar.FacialHairDetail.SESprite;

        if (pawn.GameChar.BodySprite != null)
        {
            _bodySpriteRend.sprite = pawn.GameChar.BodySprite;
            _SWFace = pawn.GameChar.SWEyesSprite;
            _SEFace = pawn.GameChar.SEEyesSprite;
        }

        if (pawn.OnPlayerTeam)
        {
            UpdateFacingAndSpriteOrder(Vector3.zero, new Vector3(1, 1), pawn.CurrentTile);
            _anim.Play("IdleNE", 0, Random.Range(0f, 1f));
        }
        else
        {
            UpdateFacingAndSpriteOrder(Vector3.zero, new Vector3(-1, -1), pawn.CurrentTile);
            _anim.Play("IdleSW", 0, Random.Range(0f, 1f));
        }
    }

    public void Reset()
    {
    
    }

    public void HandleTurnEnd()
    {
        _anim.SetBool("MyTurn", false);
    }

    public void HandleTurnBegin()
    {
        // MyTurn is just useful for triggering exit state to certain anims,
        // like IdleMyTurn, if we're still in them on turn end, without
        // explicitly calling Idle anim - so we don't interrupt another possible
        // anim
        _anim.SetBool("MyTurn", true);

        switch(_facingDirection)
        {
            case FacingDirection.NW:
                _anim.Play("ActivatedNW");
                break;
            case FacingDirection.NE:
                _anim.Play("ActivatedNE");
                break;
            case FacingDirection.SW:
                _anim.Play("ActivatedSW");
                break;
            case FacingDirection.SE:
                _anim.Play("ActivatedSE");
                break;
        }

    }

    public void UpdateFacingAnimParam()
    {
        _anim.SetBool("NE", false);
        _anim.SetBool("SE", false);
        _anim.SetBool("SW", false);
        _anim.SetBool("NW", false);

        string facingStringParam = _facingDirection switch
        {
            FacingDirection.NW => "NW",
            FacingDirection.NE => "NE",
            FacingDirection.SW => "SW",
            FacingDirection.SE => "SE",
            _ => "SE"
        };

        _anim.SetBool(facingStringParam, true);
    }

    private void UpdateFaceOrder(int newOrder)
    {
        _eyesSpriteRend.sortingOrder = newOrder + 0;
        _browSpriteRend.sortingOrder = newOrder + 0;
        // same level as helm
        _hairSpriteRend.sortingOrder = newOrder + 4;
        _fhairSpriteRend.sortingOrder = newOrder + 0;
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
            totalSpriteOrder = -(int)(currentTile.transform.position.y * 20);
        }

        // going NE
        if (facingPosition.x > originPosition.x && facingPosition.y > originPosition.y)
        {
            UpdateSpriteFacing(FacingDirection.NE);

            _bodySpriteRend.sortingOrder = totalSpriteOrder + 2;
            UpdateFaceOrder(totalSpriteOrder + 0);
        }
        // going NW
        else if (facingPosition.x < originPosition.x && facingPosition.y > originPosition.y)
        {
            UpdateSpriteFacing(FacingDirection.NW);

            _bodySpriteRend.sortingOrder = totalSpriteOrder + 2;
            UpdateFaceOrder(totalSpriteOrder + 0);
        }
        // going SE
        else if (facingPosition.x > originPosition.x && facingPosition.y < originPosition.y)
        {
            UpdateSpriteFacing(FacingDirection.SE);

            _bodySpriteRend.sortingOrder = totalSpriteOrder + 0;
            UpdateFaceOrder(totalSpriteOrder + 2);
        }
        // going SW
        else
        {
            UpdateSpriteFacing(FacingDirection.SW);

            _bodySpriteRend.sortingOrder = totalSpriteOrder + 0;
            UpdateFaceOrder(totalSpriteOrder + 2);
        }

        _headSpriteRend.sortingOrder = 1 + totalSpriteOrder;
        _weaponSpriteRend.sortingOrder = totalSpriteOrder;
        _shieldSpriteRend.sortingOrder = totalSpriteOrder + 3;
        _helmSpriteRend.sortingOrder = 4 + totalSpriteOrder;
    
        UpdateFacingAnimParam();
    }

    private void UpdateSpriteFacing(FacingDirection newDirection)
    {
        _facingDirection = newDirection;

        switch (_facingDirection)
        {
            case FacingDirection.NE:
                _eyesSpriteRend.sprite = _blankSprite;
                _hairSpriteRend.sprite = _NEHair;
                _fhairSpriteRend.sprite = _NEFacialHair;
                _browSpriteRend.sprite = _NEBrow;
                _helmSpriteRend.sprite = _currentHelm?.NESprite;
                _shieldSpriteRend.sprite = _currentShield?.NESprite;
                break;
            case FacingDirection.SE:
                _eyesSpriteRend.sprite = _SEFace;
                _hairSpriteRend.sprite = _SEHair;
                _fhairSpriteRend.sprite = _SEFacialHair;
                _browSpriteRend.sprite = _SEBrow;
                _helmSpriteRend.sprite = _currentHelm?.SESprite;
                _shieldSpriteRend.sprite = _currentShield?.SESprite;
                break;
            case FacingDirection.SW:
                _eyesSpriteRend.sprite = _SWFace;
                _hairSpriteRend.sprite = _SWHair;
                _fhairSpriteRend.sprite = _SWFacialHair;
                _browSpriteRend.sprite = _SWBrow;
                _helmSpriteRend.sprite = _currentHelm?.SWSprite;
                _shieldSpriteRend.sprite = _currentShield?.SWSprite;
                break;
            case FacingDirection.NW:
                _eyesSpriteRend.sprite = _blankSprite;
                _hairSpriteRend.sprite = _NWHair;
                _fhairSpriteRend.sprite = _NWFacialHair;
                _browSpriteRend.sprite = _NWBrow;
                _helmSpriteRend.sprite = _currentHelm?.NWSprite;
                _shieldSpriteRend.sprite = _currentShield?.NWSprite;
                break;
        }
    }

    #region Animations

    public void PlayAttack(Vector3 attackDirection)
    {
        string animationString = "AttackSW";

        if (attackDirection.x > 0 && attackDirection.y > 0)
        {
            animationString = "AttackSW";
        }
        if (attackDirection.x < 0 && attackDirection.y > 0)
        {
            animationString = "AttackSE";
        }
        if (attackDirection.x > 0 && attackDirection.y < 0)
        {
            animationString = "AttackNW";
        }
        if (attackDirection.x < 0 && attackDirection.y < 0)
        {
            animationString = "AttackNE";
        }

        _anim.Play(animationString);
    }

    public void Move()
    {
        switch (_facingDirection)
        {
            case FacingDirection.NW:
                _anim.Play("WalkNW");
                break;
            case FacingDirection.NE:
                _anim.Play("WalkNE");
                break;

            case FacingDirection.SE:
                _anim.Play("WalkSE");
                break;

            case FacingDirection.SW:
                _anim.Play("WalkSW");
                break;
        }
    }

    public void StopMoving()
    {
        switch (_facingDirection)
        {
            case FacingDirection.NW:
                _anim.Play("IdleNW", 0, Random.Range(0f, 1f));
                break;
            case FacingDirection.NE:
                _anim.Play("IdleNE", 0, Random.Range(0f, 1f));
                break;

            case FacingDirection.SE:
                _anim.Play("IdleSE", 0, Random.Range(0f, 1f));
                break;

            case FacingDirection.SW:
                _anim.Play("IdleSW", 0, Random.Range(0f, 1f));
                break;
        }
    }

    public void Die()
    {
        if (_facingDirection == FacingDirection.SE)
        {
            _eyesSpriteRend.sprite = _SEDeadFace;
        }
        else if (_facingDirection == FacingDirection.SW)
        {
            _eyesSpriteRend.sprite = _SWDeadFace;
        }

        StartCoroutine(PlayAnimationAfterDelay(.2f, "Die"));
        
        _hairSpriteRend.sortingLayerName = "DeadCharacters";
        _browSpriteRend.sortingLayerName = "DeadCharacters";
        _fhairSpriteRend.sortingLayerName = "DeadCharacters";
        _eyesSpriteRend.sortingLayerName = "DeadCharacters";
        _bodySpriteRend.sortingLayerName = "DeadCharacters";
        _headSpriteRend.sortingLayerName = "DeadCharacters";
        _helmSpriteRend.sortingLayerName = "DeadCharacters";
        _weaponSpriteRend.sortingLayerName = "DeadCharacters";
        _shieldSpriteRend.sortingLayerName = "DeadCharacters";
    }

    public void HandleHit(bool isDead, bool armorHit, bool armorDestroyed)
    {
        string animationString = "";
        if (armorHit)
        {
            StartCoroutine(PlayArmorHitFXAfterDelay(0f));
            switch (_facingDirection)
            {
                case FacingDirection.NW:
                    animationString = "GetHitARMRNW";
                    break;
                case FacingDirection.NE:
                    animationString = "GetHitARMRNE";
                    break;
                case FacingDirection.SE:
                    animationString = "GetHitARMRSE";
                    break;
                case FacingDirection.SW:
                    animationString = "GetHitARMRSW";
                    break;
            }
        }
        else
        {
            StartCoroutine(PlayBloodSpurtAfterDelay(0f));
            switch (_facingDirection)
            {
                case FacingDirection.NW:
                    animationString = "GetHitHPNW";
                    break;
                case FacingDirection.NE:
                    animationString = "GetHitHPNE";
                    break;
                case FacingDirection.SE:
                    animationString = "GetHitHPSE";
                    break;
                case FacingDirection.SW:
                    animationString = "GetHitHPSW";
                    break;
            }
        }

        if (!isDead)
        {
            _anim.Play(animationString);
            //StartCoroutine(PlayAnimationAfterDelay(.2f, animationString));
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

    public void TriggerDodge()
    {
        switch (_facingDirection)
        {
            case FacingDirection.NW:
                StartCoroutine(PlayAnimationAfterDelay(.2f, "DodgeNW"));
                break;
            case FacingDirection.NE:
                StartCoroutine(PlayAnimationAfterDelay(.2f, "DodgeNE"));
                break;
            case FacingDirection.SE:
                StartCoroutine(PlayAnimationAfterDelay(.2f, "DodgeSE"));
                break;
            case FacingDirection.SW:
                StartCoroutine(PlayAnimationAfterDelay(.2f, "DodgeSW"));
                break;
        }
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
