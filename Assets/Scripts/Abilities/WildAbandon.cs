using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WildAbandon", menuName = "MyScriptables/Abilities/WildAbandon")]
public class WildAbandon : SupportAbilityData
{
    private int turnsForEffect;
    private Pawn _activatedPawn;
    private Pawn _targetPawn;

    // note... it may not be necessary to have the activated pawn param. Will the pawn that this
    // ability instance belongs to ever change?
    public override bool Activate(Pawn activatedPawn, Pawn targetPawn)
    {
        _activatedPawn = activatedPawn;
        _targetPawn = targetPawn;

        // _activatedPawn.OutDamageMult = outDmgMod;
        // _activatedPawn.InDamageMult = inDmgMod;
        
        turnsForEffect = turnsDuration;

        _activatedPawn.Motivation -= motCost;

        // on new activation, will want to check duration to see if end effect
        _activatedPawn.OnTurnBegin.AddListener(HandleNewActivationForPawn);
        // ded. remove effect.
        _activatedPawn.OnHPChanged.AddListener(HandleDeath);

        // tell battle manager we acted
        FlowDirector.Instance.HandlePawnActed(_activatedPawn);
        
        // text display event
        // BattleManager.Instance.AddTextNotification(_targetPawn.transform.position, "+Raging");

        // add icon to the character UI to show effect
        _targetPawn.UpdateEffect(statusEffect, true);

        return true;
    }

    /// <summary>
    /// If pawn is killed end the effect
    /// </summary>
    private void HandleDeath()
    {
        if (_activatedPawn.IsDead)
        {
            StopEffect();
        }
    }

    /// <summary>
    /// Upon activation check if it's time to stop going rambo
    /// </summary>
    private void HandleNewActivationForPawn()
    {
        turnsForEffect--;

        // remove listener and remove protection from target pawn
        if (turnsForEffect <= 0)
        {
            StopEffect();
        }
    }

    /// <summary>
    /// Stop the activated pawn from protecting the target pawn, and cleanup
    /// </summary>
    private void StopEffect()
    {
        // BattleManager.Instance.AddTextNotification(_targetPawn.transform.position, "-Raging");

        _targetPawn.UpdateEffect(statusEffect, false);

        _activatedPawn.OnTurnBegin.RemoveListener(HandleNewActivationForPawn);
        _activatedPawn.OnHPChanged.RemoveListener(HandleDeath);

        // _activatedPawn.OutDamageMult = 0;
        // _activatedPawn.InDamageMult = 0;

        _activatedPawn = null;
        _targetPawn = null;
    }
}