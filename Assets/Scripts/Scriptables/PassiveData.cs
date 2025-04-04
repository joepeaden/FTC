using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PassiveData", menuName = "MyScriptables/PassiveData")]
public class PassiveData : ScriptableObject
{
    public string passiveID;

    [Header("Display Info")]
    public string displayName;
    public string description;
    public EffectData effectDisplay;
    
    [Header("Combat Modifiers")]
    public int damageOutModifier;
    public int damageInModifier;
    public int critRollModifier;

    [Header("Other Modifiers")]
    public int moveModifier;
}