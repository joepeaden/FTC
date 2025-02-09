using System;

public class BonusPay : Ability
{
    private int turnsForEffect;
    private Pawn _activatedPawn;
    private Pawn _targetPawn;

    public BonusPay()
    {
        dataAddress = "Assets/Scriptables/Abilities/BonusPay.asset";
        base.LoadData();
    }

    // note... it may not be necessary to have the activated pawn param. Will the pawn that this
    // ability instance belongs to ever change?
    public override bool Activate(Pawn activatedPawn, Pawn targetPawn)
    {
        _activatedPawn = activatedPawn;
        _targetPawn = targetPawn;

        _activatedPawn.DodgeMod = GetData().dodgeMod;
        _activatedPawn.HitMod = GetData().hitMod;

        _activatedPawn.Motivation -= GetData().cost;
        
        turnsForEffect = GetData().turnsDuration;

        // on new activation, will want to check duration to see if end effect
        _activatedPawn.OnActivation.AddListener(HandleNewActivationForPawn);
        // ded. remove effect.
        _activatedPawn.OnHit.AddListener(HandleDeath);

        // tell battle manager we acted
        BattleManager.Instance.PawnActivated(_activatedPawn);
        
        // text display event
        BattleManager.Instance.AddTextNotification(_targetPawn.transform.position, "+Boosted");

        // add icon to the character UI to show effect
        _targetPawn.UpdateEffect(GetData().statusEffect, true);

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
        BattleManager.Instance.AddTextNotification(_targetPawn.transform.position, "-Boosted");

        _targetPawn.UpdateEffect(GetData().statusEffect, false);

        _activatedPawn.OnActivation.RemoveListener(HandleNewActivationForPawn);
        _activatedPawn.OnHit.RemoveListener(HandleDeath);

        _activatedPawn.DodgeMod = 0;
        _activatedPawn.HitMod = 0;

        _activatedPawn = null;
        _targetPawn = null;
    }
}