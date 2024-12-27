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

    private int _totalSpriteOrder;

    private int _localBodyOrder;
    private int _localHeadOrder;
    private int _localFaceOrder;
    private int _localHelmOrder;
    private int _localWeaponOrder;

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

    private void OnDestroy()
    {
        
    }

    float dustTimer;
    private void Update()
    {
        //float animationVelocity = Player.instance.GetComponent<Rigidbody2D>().velocity.magnitude;

        //if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
        //{
        //    //Debug.Log("Playing");
        //    dustTimer -= Time.deltaTime;
        //    if (dustTimer < 0f)
        //    {
        //        moveDust.Emit(1);
        //        dustTimer = .1f;
        //    }
        //}
        //else
        //{
        //    dustTimer = .1f;
        //}

        //if (animationVelocity <= .5)
        //{
        //    // make sure it's negative so it's "less than zero" so that the anim controller knows we stopped
        //    animationVelocity = -1f;
        //    isMoving = false;
        //}
        //else if (!isMoving)
        //{
        //    isMoving = true;
        //    animator.SetFloat("AnimOffset", Random.Range(0f, 1f));
        //}

        //animator.SetFloat("Velocity", animationVelocity);


        //int newFaceSorting = 26;
        //// sort body sprites based on facing
        //if (_charBody.rotation.eulerAngles.z > 90 && _charBody.rotation.eulerAngles.z < 270)
        //{
        //    // weapon pointing down
        //    _bodySpriteRend.sortingOrder = 24;
        //}
        //else
        //{
        //    // weapon pointing up
        //    _bodySpriteRend.sortingOrder = 26;
        //}

        //if (_charBody.rotation.eulerAngles.z >= 45 && _charBody.rotation.eulerAngles.z < 135)
        //{
        //    _faceSpriteRend.sprite = leftFace;
        //}
        //else if (_charBody.rotation.eulerAngles.z >= 135 && _charBody.rotation.eulerAngles.z < 225)
        //{
        //    _faceSpriteRend.sprite = downFace;
        //}
        //else if (_charBody.rotation.eulerAngles.z >= 225 && _charBody.rotation.eulerAngles.z < 315)
        //{
        //    _faceSpriteRend.sprite = rightFace;
        //}
        //else
        //{
        //    _faceSpriteRend.sprite = upFace;
        //    newFaceSorting = 24;
        //}

        //_faceSpriteRend.sortingOrder = newFaceSorting;

        //_headSpriteRend.gameObject.transform.localPosition = _charBody.transform.up * .05f;
        //// lower the head so it doesn't stick out too much
        //_headSpriteRend.gameObject.transform.localPosition = new Vector3(_headSpriteRend.gameObject.transform.localPosition.x, _headSpriteRend.gameObject.transform.localPosition.y - .03f, _headSpriteRend.gameObject.transform.localPosition.z);
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
            UpdateFacing(Vector3.zero, new Vector3(1, 1));
            _anim.Play("IdleNE", 0, Random.Range(0f, 1f));
        }
        else
        {
            UpdateFacing(Vector3.zero, new Vector3(-1, -1));
            _anim.Play("IdleSW", 0, Random.Range(0f, 1f));
        }
        
    }

    public void Reset()
    {
    
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
    public void UpdateFacing(Vector3 originPosition, Vector3 facingPosition, Tile currentTile = null)
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

        // not sure this matters but might as well. It's updated when pawn stops moving (see StopMoving)
        _faceSpriteRend.sortingOrder = 0;
        _bodySpriteRend.sortingOrder = 0;
        _headSpriteRend.sortingOrder = 0;
        _helmSpriteRend.sortingOrder = 0;
        _weaponSpriteRend.sortingOrder = 0;
    }

    public void HandleHit(bool isDead, bool armorHit, bool armorDestroyed)
    {
        string animationString = "";
        if (armorHit)
        {
            StartCoroutine(PlayArmorHitFXAfterDelay(0f));
            animationString = "GetHitARMR";
        }
        else
        {
            StartCoroutine(PlayBloodSpurtAfterDelay(0f));
            animationString = "GetHitHP";
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
        StartCoroutine(PlayAnimationAfterDelay(.2f, "Dodge"));
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
