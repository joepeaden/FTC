using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Shove", menuName = "MyScriptables/Abilities/Shove")]
public class Shove : SupportAbilityData
{
    public override bool Activate(Pawn activatedPawn, Pawn targetPawn)
    {
        activatedPawn.Motivation -= motCost;

        Tile pushTile = targetPawn.CurrentTile.GetNextTileInDirection(targetPawn.transform.position - activatedPawn.transform.position);

        if (pushTile.CanTraverse(activatedPawn))
        {
            targetPawn.ForceMoveToTile(pushTile);
            // tell battle manager we acted
            BattleManager.Instance.PawnActivated(activatedPawn);
        }

        return true;
    }
}