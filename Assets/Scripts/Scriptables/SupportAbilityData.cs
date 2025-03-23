using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "SupportAbilityData", menuName = "MyScriptables/SupportAbilityData")]
public class SupportAbilityData : Ability
{
    [Header("Support Info")]
    public int turnsDuration;
    public EffectData statusEffect;

    [Header("Mods")]
    /// <summary>
    /// Outgoing Damage
    /// </summary>
    public int outDmgMod;
    /// <summary>
    /// Incoming Damage
    /// </summary>
    public int inDmgMod;
    public float hitMod;
    public float dodgeMod;
}