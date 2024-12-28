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

    [SerializeField] private Image helmRend;
    [SerializeField] private Image weaponRend;
    [SerializeField] private Image bodyRend;

    private Pawn _pawn;

    private void OnDestroy()
    {
        OnPawnPreviewHoverStart.RemoveAllListeners();
        OnPawnPreviewHoverEnd.RemoveAllListeners();
    }

    public void SetData(GameCharacter g)
    {
        ArmorItemData helm = g.HelmItem;
        WeaponItemData weapon = g.WeaponItem;

        SetupAppearance(helm, weapon, g.BodySprite);
    }

    public void SetData(Pawn p)
    {
        _pawn = p;

        //headRend.sprite = p.GetFaceSprite();

        ArmorItemData helm = p.GameChar.HelmItem;
        // don't show helm if it's destroyed
        if (p.ArmorPoints < 0)
        {
            helm = null;
        }

        WeaponItemData weapon = p.GameChar.WeaponItem;

        SetupAppearance(helm, weapon, p.GameChar.BodySprite);
    }

    private void SetupAppearance(ArmorItemData helm, WeaponItemData weapon, Sprite bodySprite)
    {
        if (helm != null)
        {
            helmRend.gameObject.SetActive(true);
            helmRend.sprite = helm.itemSprite;
        }
        else
        {
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

        if (bodySprite != null)
        {
            bodyRend.sprite = bodySprite;
        }
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
