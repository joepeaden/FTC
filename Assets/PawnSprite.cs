using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnSprite : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer faceSpriteRend;
    [SerializeField]
    SpriteRenderer headSpriteRend;
    [SerializeField]
    SpriteRenderer bodySpriteRend;
    [SerializeField]
    Transform charBody;
    [SerializeField]
    Sprite headHitSprite;
    [SerializeField]
    Sprite bodyHitSprite;

    [SerializeField]
    Sprite upFace;
    [SerializeField]
    Sprite downFace;
    [SerializeField]
    Sprite leftFace;
    [SerializeField]
    Sprite rightFace;

    Animator animator;
    bool isMoving;

    Sprite oldBodySprite;
    Sprite oldHeadSprite;


    [SerializeField] ParticleSystem moveDust;

    void Start()
    {
        animator = GetComponent<Animator>();

        oldBodySprite = bodySpriteRend.sprite;
        oldHeadSprite = headSpriteRend.sprite;
    }

    float dustTimer;
    private void Update()
    {
        float animationVelocity = Player.instance.GetComponent<Rigidbody2D>().velocity.magnitude;

        if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
        {
            //Debug.Log("Playing");
            dustTimer -= Time.deltaTime;
            if (dustTimer < 0f)
            {
                moveDust.Emit(1);
                dustTimer = .1f;
            }
        }
        else
        {
            dustTimer = .1f;
        }

        if (animationVelocity <= .5)
        {
            // make sure it's negative so it's "less than zero" so that the anim controller knows we stopped
            animationVelocity = -1f;
            isMoving = false;
        }
        else if (!isMoving)
        {
            isMoving = true;
            animator.SetFloat("AnimOffset", Random.Range(0f, 1f));
        }

        animator.SetFloat("Velocity", animationVelocity);


        int newFaceSorting = 26;
        // sort body sprites based on facing
        if (charBody.rotation.eulerAngles.z > 90 && charBody.rotation.eulerAngles.z < 270)
        {
            // weapon pointing down
            bodySpriteRend.sortingOrder = 24;
        }
        else
        {
            // weapon pointing up
            bodySpriteRend.sortingOrder = 26;
        }

        if (charBody.rotation.eulerAngles.z >= 45 && charBody.rotation.eulerAngles.z < 135)
        {
            faceSpriteRend.sprite = leftFace;
        }
        else if (charBody.rotation.eulerAngles.z >= 135 && charBody.rotation.eulerAngles.z < 225)
        {
            faceSpriteRend.sprite = downFace;
        }
        else if (charBody.rotation.eulerAngles.z >= 225 && charBody.rotation.eulerAngles.z < 315)
        {
            faceSpriteRend.sprite = rightFace;
        }
        else
        {
            faceSpriteRend.sprite = upFace;
            newFaceSorting = 24;
        }

        faceSpriteRend.sortingOrder = newFaceSorting;

        headSpriteRend.gameObject.transform.localPosition = charBody.transform.up * .05f;
        // lower the head so it doesn't stick out too much
        headSpriteRend.gameObject.transform.localPosition = new Vector3(headSpriteRend.gameObject.transform.localPosition.x, headSpriteRend.gameObject.transform.localPosition.y - .03f, headSpriteRend.gameObject.transform.localPosition.z);
    }

    public void HandleHit()
    {
        StartCoroutine(FlashHit());
    }

    IEnumerator FlashHit()
    {
        faceSpriteRend.enabled = false;
        bodySpriteRend.sprite = bodyHitSprite;
        headSpriteRend.sprite = headHitSprite;

        yield return new WaitForSeconds(.1f);

        faceSpriteRend.enabled = true;
        bodySpriteRend.sprite = oldBodySprite;
        headSpriteRend.sprite = oldHeadSprite;
    }

    public void Reset()
    {
        faceSpriteRend.enabled = true;
        if (oldBodySprite != null)
        {
            bodySpriteRend.sprite = oldBodySprite;
        }
        if (oldHeadSprite != null)
        {
            headSpriteRend.sprite = oldHeadSprite;
        }
    }
}
}
