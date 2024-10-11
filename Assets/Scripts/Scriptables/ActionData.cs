using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "ActionData", menuName = "MyScriptables/ActionData")]
public class ActionData : ScriptableObject
{
    public enum AttackStyle
    {
        Basic,
        LShape
    }

    public int motCost;
    public int apCost;
    public string actionName;
    public string description;
    public int range;
    public int damageMod;
    public float armorDamageMod;
    public float penetrationDamageMod;
    public float accMod;
    public int rangeForExtraDamage;
    public float extraDmgMultiplier = -1f;
    public AttackStyle attackStyle;
}