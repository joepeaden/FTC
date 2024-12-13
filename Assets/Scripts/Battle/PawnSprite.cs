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

    #region UnityEvents

    private void Start()
    {
        _anim = GetComponent<Animator>();

        _anim.Play("Idle");
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

    public void SetData(GameCharacter _gameChar)
    {
        if (_gameChar.WeaponItem != null)
        {
            _weaponSpriteRend.sprite = _gameChar.WeaponItem.itemSprite;
        }

        if (_gameChar.HelmItem != null)
        {
            _helmSpriteRend.sprite = _gameChar.HelmItem.itemSprite;
        }
    }
    public void Reset()
    {
    
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
    public void UpdateFacing(Vector3 originPosition, Vector3 facingPosition)
    {
        // going NE
        if (facingPosition.x > originPosition.x && facingPosition.y > originPosition.y)
        {
            _faceSpriteRend.sprite = _NEFace;
            _bodySpriteRend.sortingOrder = 2;
            _faceSpriteRend.sortingOrder = 0;
        }
        // going NW
        else if (facingPosition.x < originPosition.x && facingPosition.y > originPosition.y)
        {
            _faceSpriteRend.sprite = _NWFace;
            _bodySpriteRend.sortingOrder = 2;
            _faceSpriteRend.sortingOrder = 0;
        }
        // going SE
        else if (facingPosition.x > originPosition.x && facingPosition.y < originPosition.y)
        {
            _faceSpriteRend.sprite = _SEFace;
            _bodySpriteRend.sortingOrder = 0;
            _faceSpriteRend.sortingOrder = 2;
        }
        // going SW
        else
        {
            _faceSpriteRend.sprite = _SWFace;
            _bodySpriteRend.sortingOrder = 0;
            _faceSpriteRend.sortingOrder = 2;
        }
    }

    #region Animations

    public void PlayAttack()
    {
        _anim.Play("Attack");
    }

    public void Move()
    {
        _anim.Play("WobbleWalk");
    }

    public void StopMoving()
    {
        _anim.Play("Idle");
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
        if (armorHit)
        {
            StartCoroutine(PlayArmorHitFXAfterDelay(.32f));
            if (!isDead)
            {
                StartCoroutine(PlayAnimationAfterDelay(.2f, "GetArmorHit"));
            }
        }
        else
        {
            StartCoroutine(PlayBloodSpurtAfterDelay(.32f));
            if (!isDead)
            {
                StartCoroutine(PlayAnimationAfterDelay(.2f, "GetHit"));
            }
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
