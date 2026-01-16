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
            return false;
        }
         
        if (activatedPawn.freeAttacksRemaining > 0)
        {
            activatedPawn.freeAttacksRemaining--;
        }
        else
        {
            activatedPawn.actionPoints -= apCost;
        }
        activatedPawn.Motivation -= motCost;

        activatedPawn.AttackPawn(targetPawn, this);

        BattleManager.Instance.HandlePawnActed(activatedPawn);

        activatedPawn.SetSpriteFacing(targetPawn.transform.position);
        
        return true;
    }
}