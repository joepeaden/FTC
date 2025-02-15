using System;
using System.Collections.Generic;

public class BasicAttackAbility : Ability
{
    public BasicAttackAbility(string newDataAddress)
    {
        dataAddress = newDataAddress;
        base.LoadData();
    }

    public override bool Activate(Pawn activatedPawn, Pawn targetPawn)
    {
        WeaponAbilityData attackAction = GetData() as WeaponAbilityData;

        if (activatedPawn.ActionPoints < attackAction.apCost || activatedPawn.Motivation < attackAction.motCost)
        {
            // why is PawnActivated called here? That seems pretty wierd. Doesn't seem necessary.
            // The pawn isn't acting, it can't because there's not enough resources.
            BattleManager.Instance.PawnActivated(activatedPawn);

            return false;
        }
         
        activatedPawn.AttackPawn(targetPawn, attackAction);

        activatedPawn.ActionPoints -= attackAction.apCost;
        activatedPawn.Motivation -= attackAction.motCost;

        BattleManager.Instance.PawnActivated(activatedPawn);

        activatedPawn.SetSpriteFacing(targetPawn.transform.position);
        
        return true;
    }
}