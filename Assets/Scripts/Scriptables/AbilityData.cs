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
    public float hitMod;
    public float dodgeMod;
    public int range;
    public AudioClip soundEffect;
    public EffectData statusEffect;
    public Sprite sprite;
}