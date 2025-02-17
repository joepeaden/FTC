using UnityEngine;

/// <summary>
/// Scriptable for Motivation Conditions, like Oaths.
/// </summary>
[CreateAssetMenu(fileName = "MotCondData", menuName = "MyScriptables/MotCondData")]
public class MotCondData : ScriptableObject
{
    public enum ConditionType
    {
        AlliesCantTakeDamage,
        KillOneEnemy,
        DoNotDisengage
    }

    /// <summary>
    /// Unique ID for conditions
    /// </summary>
    public string condId;
    public string condName;
    public string description;
    public int tier;
    public ConditionType condType;
}