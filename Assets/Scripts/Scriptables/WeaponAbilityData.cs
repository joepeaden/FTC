using UnityEngine;

[CreateAssetMenu(fileName = "WeaponAbilityData", menuName = "MyScriptables/WeaponAbilityData")]
public class WeaponAbilityData : Ability
{
    public enum AttackStyle
    {
        Radial,        // Attacks all tiles within range (no directional restriction)
        StraightLine,  // Attacks only in a straight line in the facing direction
        LSweep,        // Attacks in an L-shape pattern
        Cone,           // Attacks in a cone/quadrant in front of the pawn
        Area
    }

    [Header("Resources")]
    public int apCost;

    [Header("Mods")]
    public int critChanceMod;
    public int rangeForExtraDamage;
    public int bonusDmg;

    [Header("Attack Pattern")]
    public AttackStyle attackStyle = AttackStyle.Cone;

    /// <summary>
    /// Gets the attack pattern instance for this weapon ability.
    /// </summary>
    public AttackPattern GetAttackPattern()
    {
        return attackStyle switch
        {
            AttackStyle.StraightLine => new StraightLineAttackPattern(),
            AttackStyle.LSweep => new LSweepAttackPattern(),
            AttackStyle.Cone => new ConeAttackPattern(),
            AttackStyle.Radial => new RadialAttackPattern(),
            AttackStyle.Area => new AreaAttackPattern(),
            _ => new ConeAttackPattern() // Default to cone
        };
    }
}
