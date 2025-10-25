using System;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "DefensivePosture", menuName = "MyScriptables/Abilities/DefensivePosture")]
public class DefensivePosture : SupportAbilityData
{
    private int _remainingDuration;
    private Pawn _activatedPawn;
    public override bool Activate(Pawn activatedPawn, Pawn targetPawn)
    {
        _activatedPawn = activatedPawn;

        _activatedPawn.Motivation -= motCost;

        _activatedPawn.InDefensiveStance = true;
        _remainingDuration = turnsDuration;

        activatedPawn.DodgeMod += dodgeMod;

        _activatedPawn.OnActivation.AddListener(HandleNewActivationForPawn);
        _activatedPawn.OnHPChanged.AddListener(HandleDeath);

        // tell battle manager we acted
        BattleManager.Instance.PawnActivated(_activatedPawn);
        
        // text display event
        BattleManager.Instance.AddPendingTextNotification("Defensive Stance", Color.white, activatedPawn.transform.position);

        return true;
    }

    /// <summary>
    /// If pawn is killed whilist protecting another, end the protection
    /// </summary>
    private void HandleDeath()
    {
        if (_activatedPawn.IsDead)
        {
            StopDefensiveStance();
        }
    }

    /// <summary>
    /// Upon activation check if we're done protecting that mofo
    /// </summary>
    private void HandleNewActivationForPawn()
    {
        _remainingDuration--;

        // remove listener and remove protection from target pawn
        if (_remainingDuration <= 0)
        {
            StopDefensiveStance();
        }
    }

    /// <summary>
    /// Stop the activated pawn from protecting the target pawn, and cleanup
    /// </summary>
    private void StopDefensiveStance()
    {
        if (_activatedPawn == null)
        {
            return;
        }
        
        _activatedPawn.OnActivation.RemoveListener(HandleNewActivationForPawn);
        _activatedPawn.OnHPChanged.RemoveListener(HandleDeath);
        _activatedPawn.InDefensiveStance = false;
        _activatedPawn.DodgeMod -= dodgeMod;
        _activatedPawn = null;
    }
}