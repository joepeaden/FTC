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

    /// <summary>
    /// For abilities which do not have a target; they should activate when
    /// the button is clicked.
    /// </summary>
    public bool immediateActivate;

    // for now - sprint for example is a move ability. The tile highlight
    // code needs to know so that it knows how to highlight.
    // This could be in the SupportAbility code but this way I don't have
    // to cast it every time to a SupportAbility to check if this is true.
    public bool isMoveAbility;
}