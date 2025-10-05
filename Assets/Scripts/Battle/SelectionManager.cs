using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SelectionManager : MonoBehaviour
{
    public Tile SelectedTile => _selectedTile;
    private Tile _selectedTile;

    private InputState _currentInputState;

    public readonly InactiveState inactiveState = new();
    public readonly IdleState idleState = new();
    public readonly MovingState movingState = new();
    public readonly UsingAbilityState abilityState = new();
    public readonly SetFacingState facingState = new();

    // the following couple variables are input for states
    public bool PlayerControlsActive { get; set; }
    public bool PlayerLeftClick { get; set; }
    public bool PlayerRightClick { get; set; }
    public Pawn CurrentPawn { get; set; }
    public Ability CurrentAbility { get; set; }
    public Tile ClickedTile { get; set; }

    private void Start()
    {
        inactiveState.TheSelectionManager = this;
        idleState.TheSelectionManager = this;
        movingState.TheSelectionManager = this;
        abilityState.TheSelectionManager = this;
        facingState.TheSelectionManager = this;

        _currentInputState = idleState;
    }

    public void SetSelectedTile(Tile newTile)
    {
        if (_selectedTile != null)
        {
            _selectedTile.SetSelected(false);
            ClearHighlights();
        }

        _selectedTile = newTile;
        _selectedTile.SetSelected(true);

        // if the character has moved here during it's turn and is not done yet (not a fresh pawn)
        if (CurrentPawn.actionPoints < Pawn.BASE_ACTION_POINTS && CurrentPawn.HasActionsRemaining())
        {
            _selectedTile.HighlightTilesInRange(CurrentPawn, CurrentPawn.MoveRange, true, Tile.TileHighlightType.Move);
        }
    }

    public void HandleTurnChange(bool playerControlsActive)
    {
        this.PlayerControlsActive = playerControlsActive;
        SetSelectedTile(CurrentPawn.CurrentTile);
    }

    public void ClearHighlights()
    {
        Pawn currentPawn = CurrentPawn;

        if (Ability.SelectedAbility != null)
        {
            _selectedTile.HighlightTilesInRange(currentPawn, Ability.SelectedAbility.range, false, Tile.TileHighlightType.AttackRange);
        }

        _selectedTile.HighlightTilesInRange(currentPawn, currentPawn.MoveRange, false, Tile.TileHighlightType.Move);
    }

    private Tile GetTileSelected()
    {
        Vector3 mousePos = CameraManager.MainCamera.ScreenToWorldPoint(Input.mousePosition);
        // the terrain pieces are labeled PathfindingAvoid, so skip those.
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, -Vector3.forward, float.MaxValue, ~LayerMask.GetMask("PathfindingAvoid"));

        if (hits.Length > 0)
        {
            return hits[0].transform.GetComponent<Tile>();
        }

        return null;
    }

    private void Update()
    {
        if (!PlayerControlsActive)
        {
            return;
        }

        PlayerLeftClick = Input.GetMouseButtonDown(0);
        PlayerRightClick = Input.GetMouseButtonDown(1);

        if (PlayerLeftClick)
        {
            ClickedTile = GetTileSelected();
        }

        InputState newState;
        newState = _currentInputState.Update();

        if (newState != _currentInputState)
        {
            _currentInputState.Exit();
            _currentInputState = newState;
            _currentInputState.Enter();
        }
    }
}

public abstract class InputState
{
    public SelectionManager TheSelectionManager { get; set; }

    public abstract InputState Update();

    public abstract void Exit();

    public abstract void Enter();
}

public class InactiveState : InputState
{
    public override InputState Update()
    {
        if (TheSelectionManager.PlayerControlsActive)
        {
            return TheSelectionManager.idleState;
        }
        
        return this;
    }

    public override void Exit()
    {
        // inactive state doesn't do anything
    }

    public override void Enter()
    {
        // inactive state doesn't do anything 
    }
}

public class IdleState : InputState
{
    public override InputState Update()
    {
        if (TheSelectionManager.CurrentAbility != null)
        {
            return TheSelectionManager.abilityState;
        }
        else if (TheSelectionManager.PlayerLeftClick && TheSelectionManager.CurrentPawn.HasMovesLeft() && TheSelectionManager.ClickedTile != null)
        {
            return TheSelectionManager.movingState;
        }

        return this;
    }

    public override void Exit()
    {
        // idle state doesn't do much
    }

    public override void Enter()
    {
        // idle state doesn't do much 
    }
}

public class MovingState : InputState
{
    private bool _movingDone;

    public override InputState Update()
    {
        if (_movingDone)
        {
            return TheSelectionManager.facingState;
        }
        else if (TheSelectionManager.CurrentAbility != null)
        {
            return TheSelectionManager.abilityState;
        }
        else if (TheSelectionManager.PlayerLeftClick)
        {
            HandleAttemptToMove();
        }
        else if (TheSelectionManager.PlayerRightClick)
        {
            return TheSelectionManager.idleState;
        }

        return this;
    }

    public override void Exit()
    {
        ClearMoveHighlight();
    }

    public override void Enter()
    {
        _movingDone = false;
        TheSelectionManager.CurrentPawn.CurrentTile.HighlightTilesInRange(TheSelectionManager.CurrentPawn, TheSelectionManager.CurrentPawn.MoveRange, true, Tile.TileHighlightType.Move);
    }

    private void ClearMoveHighlight()
    {
        TheSelectionManager.CurrentPawn.CurrentTile.HighlightTilesInRange(TheSelectionManager.CurrentPawn, TheSelectionManager.CurrentPawn.MoveRange, false, Tile.TileHighlightType.Move);
    }

    private void HandleMoveComplete()
    {
        _movingDone = true;
        TheSelectionManager.CurrentPawn.OnMoved.RemoveListener(HandleMoveComplete);
    }
    
    private void HandleAttemptToMove()
    {
        Tile targetTile = TheSelectionManager.ClickedTile;
        if (targetTile != null && !targetTile.IsImpassable)
        {
            // should we even be raycasting and all this? Should we perhaps instead just report clicks from the Tile script? So that it just 
            // directly tells the selection manager what was clicked?
            // maybe later.
            // I don't think we need to check this btw.
            Pawn targetPawn = targetTile.GetPawn();
            if (TheSelectionManager.CurrentPawn.CurrentTile.GetTilesInRange(TheSelectionManager.CurrentPawn, TheSelectionManager.CurrentPawn.MoveRange).Contains(targetTile) && targetPawn == null && TheSelectionManager.SelectedTile != null)
            {
                if (TheSelectionManager.CurrentPawn.HasPathToTile(targetTile))
                {
                    ClearMoveHighlight();
                    TheSelectionManager.CurrentPawn.OnMoved.AddListener(HandleMoveComplete);
                    TheSelectionManager.CurrentPawn.TryMoveToTile(targetTile);
                }
            }
        }
    }
}

public class SetFacingState : InputState
{
    public override InputState Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        TheSelectionManager.CurrentPawn.SetFacing(mouseWorldPos);

        if (TheSelectionManager.PlayerLeftClick)
        {
            BattleManager.Instance.PawnActivated(TheSelectionManager.CurrentPawn);
            return TheSelectionManager.idleState;
        }

        return this;
    }

    public override void Exit()
    {

        //can put the UI here to show pointing - have ref to _selectedTile;    }
    }

    public override void Enter()
    {

        //can put the UI here to show pointing - have ref to _selectedTile;
    }
}

public class UsingAbilityState : InputState
{
    public override InputState Update()
    {
        if (TheSelectionManager.PlayerLeftClick)
        {
            if (HandleAttemptUseAbility())
            {
                return TheSelectionManager.idleState;
            }
        }
        else if (TheSelectionManager.PlayerRightClick)
        {
            return TheSelectionManager.idleState;
        }

        return this;
    }

    public override void Exit()
    {
        ClearAttackHighlight();
    }

    public override void Enter()
    {
        TheSelectionManager.CurrentPawn.CurrentTile.HighlightTilesInRange(TheSelectionManager.CurrentPawn, Ability.SelectedAbility.range, true, Tile.TileHighlightType.AttackRange);
    }

    private void ClearAttackHighlight()
    {
        TheSelectionManager.CurrentPawn.CurrentTile.HighlightTilesInRange(TheSelectionManager.CurrentPawn, TheSelectionManager.CurrentPawn.MoveRange, false, Tile.TileHighlightType.Move);
    }

    private bool HandleAttemptUseAbility()
    {
        Tile newTile = TheSelectionManager.ClickedTile;
        if (newTile != null && !newTile.IsImpassable)
        {
            Pawn targetPawn = newTile.GetPawn();
            if (Ability.SelectedAbility != null && targetPawn != null && TheSelectionManager.CurrentPawn.IsTargetInRange(targetPawn, Ability.SelectedAbility))
            {
                bool isAlly = targetPawn.OnPlayerTeam == TheSelectionManager.CurrentPawn.OnPlayerTeam;

                // only allow support ability use on allies or attack ability use on enemies
                // THIS NEEDS UPDATE IF WE ADD OTHER ABILITY TYPES OTHER THAN WEAPON OR SUPPORT
                if (Ability.SelectedAbility is SupportAbilityData && isAlly ||
                    Ability.SelectedAbility is WeaponAbilityData && !isAlly)
                {
                    Ability.SelectedAbility.Activate(TheSelectionManager.CurrentPawn, targetPawn);
                    return true;
                }
            }
        }
         
        return false;
    }
}