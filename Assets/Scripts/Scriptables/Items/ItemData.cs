using UnityEngine;
using System.Collections;

public class ItemData : ScriptableObject
{
    [Header("Descriptive")]
    public string itemName;
    public string description;
    public int itemPrice;
    public ItemType itemType;
    public bool isDefault;

    /// <summary>
    /// If the item is intended to be avaialable in the store,
    /// it'll have a value greater than 0. Value corresponds 
    /// to the level that the player has upgraded their store
    /// to.
    /// </summary>
    public int storeLevel;

    [Header("Display")]
    public Sprite itemSprite;

    [Header("Directional Display")]
    public Sprite SWSprite;
}

public enum ItemType
{
    Helmet,
    Shield,
    Weapon
}
