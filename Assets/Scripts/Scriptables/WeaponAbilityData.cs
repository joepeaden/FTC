using UnityEngine;

[CreateAssetMenu(fileName = "WeaponAbilityData", menuName = "MyScriptables/WeaponAbilityData")]
public class WeaponAbilityData : Ability
{
    public enum AttackStyle
    {
        Basic,
        LShape
    }

    [Header("Resources")]
    public int apCost;

    [Header("Mods")]
    public int critChanceMod;
    public int rangeForExtraDamage;
    public int bonusDmg;

    [Header("Style")]
    public AttackStyle attackStyle;
}