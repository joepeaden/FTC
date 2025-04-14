using System;
using UnityEngine;

[CreateAssetMenu(fileName = "FirstAid", menuName = "MyScriptables/Abilities/FirstAid")]
public class FirstAid : SupportAbilityData
{
    public int healAmount;

    public override bool Activate(Pawn activatedPawn, Pawn targetPawn)
    {
        activatedPawn.Motivation -= motCost;

        targetPawn.Heal(healAmount);

        // tell battle manager we acted
        BattleManager.Instance.PawnActivated(activatedPawn);
        
        return true;
    }
}