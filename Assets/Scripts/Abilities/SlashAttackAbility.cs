using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SlashAttackAbility", menuName = "MyScriptables/Abilities/SlashAttackAbility")]
public class SlashAttackAbility : WeaponAbilityData
{
    public override bool Activate(Pawn activatedPawn, Pawn primaryTargetPawn)
    {
        if (activatedPawn.actionPoints < apCost || activatedPawn.Motivation < motCost)
        {
            return false;
        }

        List<Pawn> targetPawns = new();
        targetPawns.Add(primaryTargetPawn);

        if (attackStyle == WeaponAbilityData.AttackStyle.LShape)
        {
            Tile clockwiseNextTile = activatedPawn.CurrentTile.GetClockwiseNextTile(primaryTargetPawn.CurrentTile);
            if (clockwiseNextTile.GetPawn())
            {
                targetPawns.Add(clockwiseNextTile.GetPawn());
            }
        }

        activatedPawn.actionPoints -= apCost;
        activatedPawn.Motivation -= motCost;

        foreach (Pawn targetPawn in targetPawns)
        {
            activatedPawn.AttackPawn(targetPawn, this);
        }
        
        BattleManager.Instance.HandlePawnActed(activatedPawn);

        activatedPawn.SetSpriteFacing(primaryTargetPawn.transform.position);

        return true;
    }
}