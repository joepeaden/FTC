using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameCharacterData", menuName = "MyScriptables/GameCharacterData")]
public class GameCharacterData : ScriptableObject
{
    public string charTypeID;

    [Header("Strategic Stats")]
    public string characterTypeName;
    public bool onPlayerTeam;
    public int startingGold;
    public int price;

    [Header("Combat Stats")]
    public int minInit;
    public int maxInit;
    public int minHP;
    public int maxHP;
    public int minAcc;
    public int maxAcc;

    [Header("Equipment and Abilities")]
    public WeaponItemData fallbackWeapon;
    public List<WeaponItemData> defaultWeaponOptions;
    public List<ArmorItemData> defaultArmorOptions;
    public List<Ability> abilityList;

    [Header("Appearance")]
    public Sprite shirt;
    public Sprite eyesSE;
    public Sprite eyesSW;
    public Sprite contractDisplaySprite;
}
