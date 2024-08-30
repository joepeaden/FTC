using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private TMP_Text itemPriceTxt;
    [SerializeField]
    private Image itemSpriteRend;
    [SerializeField]
    private Button theButton;

    public ItemData Item => _item;
    private ItemData _item;

    private DecisionsManager _decisionsManager;

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetData(ItemData theItem, DecisionsManager d, UnityAction<ItemUI> callback)
    {
        gameObject.SetActive(true);

        _decisionsManager = d;

        if (theItem == null)
        {
            return;
        }

        itemSpriteRend.sprite = theItem.itemSprite;

        // the character preview ItemUI intances don't have prices
        if (itemPriceTxt != null)
        {
            itemPriceTxt.text = theItem.itemPrice.ToString();
        }

        _item = theItem;
        
        theButton.onClick.AddListener(() => { callback(this); });
    }

    public void Clear()
    {
        // no reason to clear the _decisionsManager, but otherwise clear everything out

        itemSpriteRend.sprite = null;

        if (itemPriceTxt != null)
        {
            itemPriceTxt.text = "";
        }

        _item = null;
        theButton.onClick.RemoveAllListeners();
    }

    public void SetCallback(UnityAction<ItemUI> callback)
    {
        theButton.onClick.AddListener(() => { callback(this); });
    }

    public void RemoveCallbacks()
    {
        theButton.onClick.RemoveAllListeners();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _decisionsManager.HandleItemHoverStart(_item);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _decisionsManager.HandleItemHoverEnd();
    }

    private void OnDestroy()
    {
        theButton.onClick.RemoveAllListeners();
    }
}
