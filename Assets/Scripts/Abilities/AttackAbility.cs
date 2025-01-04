using System;

public class StandardAttack : Ability
{
    public StandardAttack()
    {   
        if (!data.ContainsKey(dataAddress))
        {
            base.LoadData();
        }
    }

    public override bool Activate(Pawn activatedPawn, Pawn targetPawn)
    {
        targetPawn.TakeDamage(activatedPawn, (ActionData)data[dataAddress]);
        return true;
    }
}