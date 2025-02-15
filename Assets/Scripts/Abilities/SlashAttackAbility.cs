using System;
using System.Collections.Generic;

public class SlashAttackAbility : Ability
{
    public SlashAttackAbility()
    {
        dataAddress = "Assets/Scriptables/Abilities/WeaponBased/SwordSpecial.asset";
        base.LoadData();
    }

    public override bool Activate(Pawn activatedPawn, Pawn primaryTargetPawn)
    {
        WeaponAbilityData attackAction = GetData() as WeaponAbilityData;

        if (activatedPawn.ActionPoints < attackAction.apCost || activatedPawn.Motivation < attackAction.motCost)
        {
            // why is PawnActivated called here? That seems pretty wierd. Doesn't seem necessary.
            // The pawn isn't acting, it can't because there's not enough resources.
            BattleManager.Instance.PawnActivated(activatedPawn);

            return false;
        }

        List<Pawn> targetPawns = new();
        targetPawns.Add(primaryTargetPawn);

        if (attackAction.attackStyle == WeaponAbilityData.AttackStyle.LShape)
        {
            Tile clockwiseNextTile = activatedPawn.CurrentTile.GetClockwiseNextTile(primaryTargetPawn.CurrentTile);
            if (clockwiseNextTile.GetPawn())
            {
                targetPawns.Add(clockwiseNextTile.GetPawn());
            }
        }

        foreach (Pawn targetPawn in targetPawns)
        {
            activatedPawn.AttackPawn(targetPawn, attackAction);
        }

        activatedPawn.ActionPoints -= attackAction.apCost;
        activatedPawn.Motivation -= attackAction.motCost;
        
        BattleManager.Instance.PawnActivated(activatedPawn);

        activatedPawn.SetSpriteFacing(primaryTargetPawn.transform.position);

        return true;
    }
}