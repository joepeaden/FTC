using UnityEngine;

/// <summary>
/// Scriptable for Motivation Conditions, like Oaths.
/// </summary>
[CreateAssetMenu(fileName = "MotCondData", menuName = "MyScriptables/MotCondData")]
public class MotCondData : EffectData
{
    public enum ConditionType
    {
        AlliesCantTakeDamage,
        KillOneEnemy,
        DoNotDisengage
    }

    public int tier;
    public ConditionType condType;
}