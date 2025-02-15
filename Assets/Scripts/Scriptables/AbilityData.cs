using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "AbilityData", menuName = "MyScriptables/AbilityData")]
public class AbilityData : ScriptableObject
{
    [Header("Descriptive")]
    public string abilityName;
    public string description;
    public int motCost;
    public int range;
    public Sprite sprite;
}