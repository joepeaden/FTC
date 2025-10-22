using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "ArmorItemData", menuName = "MyScriptables/ArmorItemData")]
public class ArmorItemData : ItemData
{
    public const int MAX_ARMOR_PROTECTION = 3;

    [Header("Armor Info")]
    public int protection;
    public int poise;

    [Header("Armor Mods")]
    public int moveMod;
    public int initMod;
    public GameCharacter.CharMotivators viceToMod;
    public int viceMod;

    [Header("Armor Display")]
    public Sprite SESprite;
    public Sprite SWSprite;
    public Sprite NWSprite;
    public Sprite NESprite;
}