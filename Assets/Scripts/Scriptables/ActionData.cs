using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "ActionData", menuName = "MyScriptables/ActionData")]
public class ActionData : AbilityData
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
    /// <summary>
    /// -1 for no damage
    /// </summary>
    public int bonusDmg = -1;

    [Header("Style")]
    public AttackStyle attackStyle;
}