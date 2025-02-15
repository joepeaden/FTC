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

    [Header("Display")]
    public Sprite itemSprite;
}

public enum ItemType
{
    Helmet,
    Armor,
    Weapon
}
