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

    public int apCost;
    public int range;
    public float armorDamageMod;
    public float penetrationDamageMod;
    public int rangeForExtraDamage;
    /// <summary>
    /// -1 for no damage
    /// </summary>
    public float extraDmgMultiplier = -1f;
    public AttackStyle attackStyle;
}