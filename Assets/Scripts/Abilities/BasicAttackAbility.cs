using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BasicAttackAbility", menuName = "MyScriptables/Abilities/BasicAttackAbility")]
public class BasicAttackAbility : WeaponAbilityData
{
    public override bool Activate(Pawn activatedPawn, Pawn targetPawn)
    {
        if (activatedPawn.actionPoints < apCost || activatedPawn.Motivation < motCost)
        {
            // why is PawnActivated called here? That seems pretty wierd. Doesn't seem necessary.
            // The pawn isn't acting, it can't because there's not enough resources.
            BattleManager.Instance.PawnActivated(activatedPawn);

            return false;
        }
         
        activatedPawn.AttackPawn(targetPawn, this);

        BattleManager.Instance.PawnActivated(activatedPawn);

        activatedPawn.SetSpriteFacing(targetPawn.transform.position);
        
        return true;
    }
}