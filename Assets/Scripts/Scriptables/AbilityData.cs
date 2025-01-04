using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "AbilityData", menuName = "MyScriptables/AbilityData")]
public class AbilityData : ScriptableObject
{
    public string abilityName;
    public string description;
    public int cost;
    public int turnsDuration;
    /// <summary>
    /// Outgoing Damage
    /// </summary>
    public int outDmgMod;
    /// <summary>
    /// Incoming Damage
    /// </summary>
    public int inDmgMod;
    public int hitMod;
    public int dodgeMod;
}