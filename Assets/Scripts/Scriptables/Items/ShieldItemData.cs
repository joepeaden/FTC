using UnityEngine;

[CreateAssetMenu(fileName = "ShieldItemData", menuName = "MyScriptables/ShieldItemData")]
public class ShieldItemData : ItemData
{
    [Header("Armor Info")]
    public int blockChance;

    [Header("Armor Mods")]
    public int moveMod;
    public int initMod;
}