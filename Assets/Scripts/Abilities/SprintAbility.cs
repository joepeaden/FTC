using System;

public class SprintAbility : Ability
{
    private Pawn _activatedPawn;
    private SupportAbilityData ability;

    public SprintAbility()
    {
        dataAddress = "Assets/Scriptables/Abilities/Sprint.asset";
        base.LoadData();
        ability = GetData() as SupportAbilityData;
    }

    // note... it may not be necessary to have the activated pawn param. Will the pawn that this
    // ability instance belongs to ever change?
    public override bool Activate(Pawn activatedPawn, Pawn targetPawn)
    {
        _activatedPawn.SprintBonusMoves = ability.range;
        _activatedPawn.Motivation -= GetData().motCost;
        _activatedPawn.CurrentTile.HighlightTilesInRange(_activatedPawn, _activatedPawn.MoveRange, true, Tile.TileHighlightType.Move);
        _activatedPawn.OnActivation.AddListener(ResetSprintBonus);

        return true;
    }

    private void ResetSprintBonus()
    {
        _activatedPawn.SprintBonusMoves = 0;
        _activatedPawn.OnActivation.RemoveListener(ResetSprintBonus);
    }

}