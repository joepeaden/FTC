using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DefensivePosture", menuName = "MyScriptables/Abilities/DefensivePosture")]
public class DefensivePosture : SupportAbilityData
{
    private int _remainingDuration;
    private Pawn _activatedPawn;
    private Pawn _targetPawn;

    public override bool Activate(Pawn activatedPawn, Pawn targetPawn)
    {
        activatedPawn.Motivation -= motCost;



        // tell battle manager we acted
        BattleManager.Instance.PawnActivated(activatedPawn);
        
        return true;
    }
}
//         _targetPawn.ProtectingPawn = _activatedPawn;
//         _turnsToProtect = turnsDuration;

//         _activatedPawn.Motivation -= motCost;

//         // on new activation, will want to check duration to see if stop protecting
//         _activatedPawn.OnActivation.AddListener(HandleNewActivationForPawn);
//         // obviously can't protect someone if we're dead can we?
//         _activatedPawn.OnHPChanged.AddListener(HandleDeath);
//         // listen when the pawn moves to check if we're still in range to protect
//         _activatedPawn.OnMoved.AddListener(CheckAdjacencyForProtection);
//         _targetPawn.OnMoved.AddListener(CheckAdjacencyForProtection);

//         // tell battle manager we acted
//         BattleManager.Instance.PawnActivated(_activatedPawn);
        
//         // text display event
//         // BattleManager.Instance.AddTextNotification(_targetPawn.transform.position, "+Protection");

//         // add icon to the character UI to show effect
//         _targetPawn.UpdateEffect(statusEffect, true);

//         return true;
//     }

//     /// <summary>
//     /// If pawn is killed whilist protecting another, end the protection
//     /// </summary>
//     private void HandleDeath()
//     {
//         if (_activatedPawn.IsDead)
//         {
//             StopProtection();
//         }
//     }

//     /// <summary>
//     /// Upon activation check if we're done protecting that mofo
//     /// </summary>
//     private void HandleNewActivationForPawn()
//     {
//         _turnsToProtect--;

//         // remove listener and remove protection from target pawn
//         if (_turnsToProtect <= 0)
//         {
//             StopProtection();
//         }
//     }

//     /// <summary>
//     /// Checks if the target pawn is still adjacent, if not, end protection
//     /// </summary>
//     private void CheckAdjacencyForProtection()
//     {
//         if (!_activatedPawn.GetAdjacentPawns().Contains(_targetPawn))
//         {
//             StopProtection();
//         }
//     }

//     /// <summary>
//     /// Stop the activated pawn from protecting the target pawn, and cleanup
//     /// </summary>
//     private void StopProtection()
//     {
//         // BattleManager.Instance.AddTextNotification(_targetPawn.transform.position, "-Protection");

//         _targetPawn.UpdateEffect(statusEffect, false);

//         _activatedPawn.OnActivation.RemoveListener(HandleNewActivationForPawn);
//         _activatedPawn.OnHPChanged.RemoveListener(HandleDeath);
//         _activatedPawn.OnMoved.RemoveListener(CheckAdjacencyForProtection);
//         _targetPawn.OnMoved.RemoveListener(CheckAdjacencyForProtection);

//         _targetPawn.ProtectingPawn = null;
//         _activatedPawn = null;
//         _targetPawn = null;
//     }
// }