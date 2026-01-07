using UnityEngine;

[CreateAssetMenu(fileName = "ShieldItemData", menuName = "MyScriptables/ShieldItemData")]
public class ShieldItemData : ItemData
{
    [Header("Armor Info")]
    public int blockRange;

    public AudioClip blockSound;

    [Header("Stat Mods")]
    public int moveMod;
    public int initMod;
}