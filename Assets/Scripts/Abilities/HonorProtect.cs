using System;

public class HonorProtect : Ability
{
    private int _turnsToProtect;
    private Pawn _activatedPawn;
    private Pawn _targetPawn;

    public HonorProtect()
    {
        dataAddress = "Assets/Scriptables/Abilities/HonorProtect.asset";
        if (!data.ContainsKey(dataAddress))
        {
            base.LoadData();
        }
    }

    public override bool Activate(Pawn activatedPawn, Pawn targetPawn)
    {
        // can't protect the same pawn with more than one pawn.
        if (_targetPawn.ProtectingPawn != null)
        {
            return false;
        }

        _activatedPawn = activatedPawn;
        _targetPawn = targetPawn;

        targetPawn.ProtectingPawn = activatedPawn;

        activatedPawn.OnActivation.AddListener(HandleNewActivationForPawn);
        activatedPawn.OnHit.AddListener(HandleDeath);

        _turnsToProtect = data[dataAddress].turnsDuration;

        return true;
    }

    /// <summary>
    /// If pawn is killed whilist protecting another, end the protection
    /// </summary>
    private void HandleDeath()
    {
        if (_activatedPawn.IsDead)
        {
            StopProtection();
        }
    }

    /// <summary>
    /// Upon activation check if we're done protecting that mofo
    /// </summary>
    private void HandleNewActivationForPawn()
    {
        _turnsToProtect--;

        // remove listener and remove protection from target pawn
        if (_turnsToProtect <= 0)
        {
            StopProtection();
        }
    }

    private void StopProtection()
    {
        _activatedPawn.OnActivation.RemoveListener(HandleNewActivationForPawn);
        _activatedPawn.OnHit.RemoveListener(HandleDeath);

        _targetPawn.ProtectingPawn = null;
        _activatedPawn = null;
        _targetPawn = null;
    }
}