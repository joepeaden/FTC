using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SlashAttackAbility", menuName = "MyScriptables/Abilities/SlashAttackAbility")]
public class SlashAttackAbility : WeaponAbilityData
{
    public override bool Activate(Pawn activatedPawn, Pawn primaryTargetPawn)
    {
        if (activatedPawn.ActionPoints < apCost || activatedPawn.Motivation < motCost)
        {
            // why is PawnActivated called here? That seems pretty wierd. Doesn't seem necessary.
            // The pawn isn't acting, it can't because there's not enough resources.
            BattleManager.Instance.PawnActivated(activatedPawn);

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

        foreach (Pawn targetPawn in targetPawns)
        {
            activatedPawn.AttackPawn(targetPawn, this);
        }

        activatedPawn.ActionPoints -= apCost;
        activatedPawn.Motivation -= motCost;
        
        BattleManager.Instance.PawnActivated(activatedPawn);

        activatedPawn.SetSpriteFacing(primaryTargetPawn.transform.position);

        return true;
    }
}