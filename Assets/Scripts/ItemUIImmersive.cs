using UnityEngine;

public class ItemUIImmersive : MonoBehaviour
{
    public bool PlayerOwns = false;

    // components
    [SerializeField] private SpriteRenderer _itemSpriteRend;

    public ItemData Item => _item;
    private ItemData _item;

    public Tile CurrentTile;

    private void Awake()
    {
        _itemSpriteRend = GetComponent<SpriteRenderer>();
    }

    public void SetData(ItemData theItem, bool playerOwns)
    {
        if (_itemSpriteRend == null)
        {
            _itemSpriteRend = GetComponent<SpriteRenderer>();
        }

        PlayerOwns = playerOwns;
        // if (theItem == null)
        // {
        //     return;
        // }

        _itemSpriteRend.sprite = theItem.itemSprite;

        // the character preview ItemUI intances don't have prices
        // if (itemPriceTxt != null)
        // {
        //     itemPriceTxt.text = theItem.itemPrice.ToString();
        // }

        _item = theItem;        
    }
}
