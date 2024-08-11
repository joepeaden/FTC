using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private TMP_Text itemPriceTxt;
    [SerializeField]
    private Image itemSpriteRend;

    public ItemData Item => _item;
    private ItemData _item;

    private DecisionsManager _decisionsManager;

    public void SetData(ItemData theItem, DecisionsManager d)
    {
        itemSpriteRend.sprite = theItem.itemSprite;
        itemPriceTxt.text = theItem.itemPrice.ToString();
        _item = theItem;

        _decisionsManager = d;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _decisionsManager.HandleItemHoverStart(_item);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _decisionsManager.HandleItemHoverEnd();
    }
}
