using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PassiveData", menuName = "MyScriptables/PassiveData")]
public class PassiveData : ScriptableObject
{
    public string passiveID;
    /// <summary>
    /// Level at which the passive is available
    /// </summary>
    public int level;

    [Header("Display Info")]
    public string displayName;
    public string description;
    public EffectData effectDisplay;
    
    [Header("Combat Modifiers")]
    public int damageOutModifier;
    public int damageInModifier;
    public int critRollModifier;
    public int hitRollModifier;
    public bool damageSelfOnMiss;
    public bool downgradesCrits;

    [Header("Other Modifiers")]
    public int moveModifier;
    public int possessionChanceRoll = -1;
    public int selfHealPerTurn;

    [Header("Behaviour Modifiers")]
    public bool noRetreat;
    public bool freeAttacksPerEnemy;
    public bool oneAttackPerTurn;
    public bool obsorbDmgFromAdjacentAlly;
}