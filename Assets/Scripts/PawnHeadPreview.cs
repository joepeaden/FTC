using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PawnHeadPreview : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector]
    public UnityEvent<Pawn> OnPawnPreviewHoverStart = new();
    [HideInInspector]
    public UnityEvent OnPawnPreviewHoverEnd = new();

    [SerializeField] private Image helmRend;
    [SerializeField] private Image weaponRend;

    private Pawn _pawn;

    private void OnDestroy()
    {
        OnPawnPreviewHoverStart.RemoveAllListeners();
        OnPawnPreviewHoverEnd.RemoveAllListeners();
    }

    public void SetData(Pawn p)
    {
        _pawn = p;

        //headRend.sprite = p.GetFaceSprite();

        ArmorItemData helm = p.GameChar.HelmItem;
        if (helm != null && p.ArmorPoints > 0)
        {
            helmRend.gameObject.SetActive(true);
            helmRend.sprite = helm.itemSprite;
        }
        else
        {
            helmRend.gameObject.SetActive(false);
        }

        WeaponItemData weapon = p.GameChar.WeaponItem;
        if (weapon != null)
        {
            weaponRend.gameObject.SetActive(true);
            weaponRend.sprite = weapon.itemSprite;
        }
        else
        {
            weaponRend.gameObject.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnPawnPreviewHoverStart.Invoke(_pawn);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnPawnPreviewHoverEnd.Invoke();
    }
}
