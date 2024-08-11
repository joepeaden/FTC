using UnityEngine;
using System.Collections;

public class ItemData : ScriptableObject
{
    public string itemName;
    public string description;
    public Sprite itemSprite;
    public int itemPrice;
    public ItemType itemType;
}

public enum ItemType
{
    Helmet,
    Armor,
    Weapon
}
