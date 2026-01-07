using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PawnPreview : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// Used for indicating which pawn is next
    /// </summary>
    [HideInInspector]
    public UnityEvent<Pawn> OnPawnPreviewHoverStart = new();
    [HideInInspector]
    public UnityEvent OnPawnPreviewHoverEnd = new();

    [Header("Simple Preview")]
    [SerializeField] private GameObject simplePreview;
    [SerializeField] private Image simplePreviewImage;

    [Header("Detailed Preview")]
    [SerializeField] private GameObject detailedPreview;
    [SerializeField] private Image helmRend;
    [SerializeField] private Image weaponRend;
    [SerializeField] private Image shieldRend;
    [SerializeField] private Image bodyRend;
    [SerializeField] private Image hairRend;
    [SerializeField] private Image eyesRend;
    [SerializeField] private Image fHairRend;
    [SerializeField] private Image browRend;

    private Pawn _pawn;

    private void OnDestroy()
    {
        OnPawnPreviewHoverStart.RemoveAllListeners();
        OnPawnPreviewHoverEnd.RemoveAllListeners();
    }

    public void SetData(Sprite displaySprite)
    {
        simplePreview.SetActive(true);
        detailedPreview.SetActive(false);

        simplePreviewImage.sprite = displaySprite;
    }

    public void SetData(GameCharacter g)
    {
        simplePreview.SetActive(false);
        detailedPreview.SetActive(true);

        ShieldItemData shield = g.ShieldItem;
        ArmorItemData helm = g.HelmItem;
        WeaponItemData weapon = g.TheWeapon.Data;

        SetupAppearance(helm, weapon, shield, g.EyesSprite, g.BodySprite, g.HairDetail.sprite, g.FacialHairDetail.sprite, g.BrowDetail.sprite);
    }

    public void SetData(Pawn p)
    {
        simplePreview.SetActive(false);
        detailedPreview.SetActive(true);

        _pawn = p;

        ArmorItemData helm = p.GameChar.HelmItem;
        // don't show helm if it's destroyed
        if (p.ArmorPoints < 0)
        {
            helm = null;
        }

        WeaponItemData weapon = p.GameChar.TheWeapon.Data;
        ShieldItemData shield = p.GameChar.ShieldItem;

        SetupAppearance(helm, weapon, shield, p.GameChar.EyesSprite, p.GameChar.BodySprite, p.GameChar.HairDetail.sprite, p.GameChar.FacialHairDetail.sprite, p.GameChar.BrowDetail.sprite);
    }

    private void SetupAppearance(ArmorItemData helm, WeaponItemData weapon, ShieldItemData shield, Sprite eyesSprite, Sprite bodySprite, Sprite hairSprite, Sprite fHairSprite, Sprite browSprite)
    {
        if (helm != null)
        {
            helmRend.gameObject.SetActive(true);
            helmRend.sprite = helm.itemSprite;
            hairRend.gameObject.SetActive(false);
        }
        else
        {
            hairRend.gameObject.SetActive(true);
            helmRend.gameObject.SetActive(false);
        }

        if (weapon != null)
        {
            weaponRend.gameObject.SetActive(true);
            weaponRend.sprite = weapon.itemSprite;
        }
        else
        {
            weaponRend.gameObject.SetActive(false);
        }

        if (shield != null)
        {
            shieldRend.gameObject.SetActive(true);
            shieldRend.sprite = shield.itemSprite;
        }
        else
        {
            shieldRend.gameObject.SetActive(false);
        }

        if (bodySprite != null)
        {
            bodyRend.sprite = bodySprite;
        }

        eyesRend.sprite = eyesSprite;
        hairRend.sprite = hairSprite;
        fHairRend.sprite = fHairSprite;
        browRend.sprite = browSprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // pawn may be null if it's setup with a gamechar, not a pawn -
        // like in the strategy layer (Decisions).
        if (_pawn != null)
        {
            OnPawnPreviewHoverStart.Invoke(_pawn);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_pawn != null)
        {
            OnPawnPreviewHoverEnd.Invoke();
        }
    }
}
