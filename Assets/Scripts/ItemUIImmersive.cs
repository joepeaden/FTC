using UnityEngine;

public class ItemUIImmersive : TileInhabitant
{
    public bool PlayerOwns = false;

    // components
    [SerializeField] private SpriteRenderer _itemSpriteRend;

    public ItemData Item => _item;
    private ItemData _item;

    private void Awake()
    {
        _itemSpriteRend = GetComponent<SpriteRenderer>();
        TheInhabitantType = InhabitantType.Item;
    }

    public void SetData(ItemData theItem, bool playerOwns)
    {
        if (_itemSpriteRend == null)
        {
            _itemSpriteRend = GetComponent<SpriteRenderer>();
        }

        PlayerOwns = playerOwns;

        _itemSpriteRend.sprite = theItem.itemSprite;

        _item = theItem;        
    }
}
