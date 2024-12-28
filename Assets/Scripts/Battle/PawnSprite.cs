using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnSprite : MonoBehaviour
{
    //[Header("Visuals")]
    [SerializeField] private Animator _anim;

    [SerializeField]
    private SpriteRenderer _faceSpriteRend;
    [SerializeField]
    private SpriteRenderer _headSpriteRend;
    [SerializeField]
    private SpriteRenderer _bodySpriteRend;
    [SerializeField]
    private SpriteRenderer _helmSpriteRend;
    [SerializeField]
    private SpriteRenderer _weaponSpriteRend;

    [SerializeField]
    private Sprite _NEFace;
    [SerializeField]
    private Sprite _NWFace;
    [SerializeField]
    private Sprite _SWFace;
    [SerializeField]
    private Sprite _SEFace;

    [SerializeField] 
    private ParticleSystem _moveDust;

    private ArmorItemData _currentHelm;

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

        if (pawn.GameChar.WeaponItem != null)
        {
            _weaponSpriteRend.sprite = pawn.GameChar.WeaponItem.itemSprite;
        }

        if (pawn.GameChar.HelmItem != null)
        {
            _helmSpriteRend.sprite = _currentHelm.SWSprite;
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

        if (pawn.GameChar.BodySprite != null)
        {
            _bodySpriteRend.sprite = pawn.GameChar.BodySprite;
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

    public void SetNewFacingAnimParam(string newFacing)
    {
        _anim.SetBool("NE", false);
        _anim.SetBool("SE", false);
        _anim.SetBool("SW", false);
        _anim.SetBool("NW", false);

        _anim.SetBool(newFacing, true);
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
            _facingDirection = FacingDirection.NE;
            SetNewFacingAnimParam("NE");
            _faceSpriteRend.sprite = _NEFace;
            if (_currentHelm != null)
            {
                _helmSpriteRend.sprite = _currentHelm.NESprite;
            }
            _bodySpriteRend.sortingOrder = totalSpriteOrder + 2;
            _faceSpriteRend.sortingOrder = totalSpriteOrder + 0;

            //_weaponSpriteRend.transform.localPosition = new Vector3(0.4f, 0.37f, 0);
            //_weaponSpriteRend.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 20));
        }
        // going NW
        else if (facingPosition.x < originPosition.x && facingPosition.y > originPosition.y)
        {
            _facingDirection = FacingDirection.NW;
            SetNewFacingAnimParam("NW");
            _faceSpriteRend.sprite = _NWFace;
            if (_currentHelm != null)
            {
                _helmSpriteRend.sprite = _currentHelm.NWSprite;
            }
            _bodySpriteRend.sortingOrder = totalSpriteOrder + 2;
            _faceSpriteRend.sortingOrder = totalSpriteOrder + 0;

            //_weaponSpriteRend.transform.localPosition = new Vector3(-0.4f, 0.37f, 0);
            //_weaponSpriteRend.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 20));
        }
        // going SE
        else if (facingPosition.x > originPosition.x && facingPosition.y < originPosition.y)
        {
            _facingDirection = FacingDirection.SE;
            SetNewFacingAnimParam("SE");
            _faceSpriteRend.sprite = _SEFace;
            if (_currentHelm != null)
            {
                _helmSpriteRend.sprite = _currentHelm.SESprite;
            }
            _bodySpriteRend.sortingOrder = totalSpriteOrder + 0;
            _faceSpriteRend.sortingOrder = totalSpriteOrder + 2;

            //_weaponSpriteRend.transform.localPosition = new Vector3(0.4f, 0.37f, 0);
            //_weaponSpriteRend.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 20));
        }
        // going SW
        else
        {
            _facingDirection = FacingDirection.SW;
            SetNewFacingAnimParam("SW");
            _faceSpriteRend.sprite = _SWFace; 
            if (_currentHelm != null)
            {
                _helmSpriteRend.sprite = _currentHelm.SWSprite;
            }
            _bodySpriteRend.sortingOrder = totalSpriteOrder + 0;
            _faceSpriteRend.sortingOrder = totalSpriteOrder + 2;

            //_weaponSpriteRend.transform.localPosition = new Vector3(-0.4f, 0.37f, 0);
            //_weaponSpriteRend.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 20));
        }

        _headSpriteRend.sortingOrder = 1 + totalSpriteOrder;
        _weaponSpriteRend.sortingOrder = totalSpriteOrder;
        _helmSpriteRend.sortingOrder = 4 + totalSpriteOrder;
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
        StartCoroutine(PlayAnimationAfterDelay(.2f, "Die"));
        _faceSpriteRend.sortingLayerName = "DeadCharacters";
        _bodySpriteRend.sortingLayerName = "DeadCharacters";
        _headSpriteRend.sortingLayerName = "DeadCharacters";
        _helmSpriteRend.sortingLayerName = "DeadCharacters";
        _weaponSpriteRend.sortingLayerName = "DeadCharacters";
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
