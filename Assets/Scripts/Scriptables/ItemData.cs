using UnityEngine;
using System.Collections;

public class ItemData : ScriptableObject
{
    public string itemName;
    public string description;
    /// <summary>
    /// display sprite.
    /// </summary>
    public Sprite itemSprite;
    public Sprite SESprite;
    public Sprite SWSprite;
    public Sprite NWSprite;
    public Sprite NESprite;
    public int itemPrice;
    public ItemType itemType;
    public bool isDefault;
}

public enum ItemType
{
    Helmet,
    Armor,
    Weapon
}
