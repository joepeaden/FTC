using System.Linq;
using UnityEngine;

// This class should be considered a sub-component of BattleManger - we shouldn't be referencing
// this much elsewhere.  

public class MyInputManager : MonoBehaviour
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
    // really, they shouldn't be set on and off outside of this class in such an uncontrolled manner.
    public bool PlayerControlsActive { get; set; }
    public bool PlayerLeftClick { get; set; }
    public bool PlayerRightClick { get; set; }
    public Pawn CurrentPawn { get; set; }
    public Ability CurrentAbility { get; set; }
    public Tile ClickedTile { get; set; }
    public Tile HoveredTile { get; set; }
    public Tile LastHoveredTile { get; set; }

    private void Start()
    {
        inactiveState.InputManager = this;
        idleState.InputManager = this;
        movingState.InputManager = this;
        abilityState.InputManager = this;
        facingState.InputManager = this;

        _currentInputState = idleState;
    }

    public void SetSelectedTile(Tile newTile)
    {
        if (_selectedTile != null)
        {
            _selectedTile.SetSelected(false);
            // ClearHighlights();
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

    public void UpdateTileHovered(Tile hoveredTile)
    {
        if (this.HoveredTile != null)
        {
            this.LastHoveredTile = this.HoveredTile;
        }

        this.HoveredTile = hoveredTile;
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
    public MyInputManager InputManager { get; set; }

    public abstract InputState Update();

    public abstract void Exit();

    public abstract void Enter();
}

public class InactiveState : InputState
{
    public override InputState Update()
    {
        if (this.InputManager.PlayerControlsActive)
        {
            return this.InputManager.idleState;
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
    public Tile lastTileHighlightedFor = null;
    public override InputState Update()
    {
        // if (this.InputManager.HoveredTile != null && lastTileHighlightedFor != this.InputManager.HoveredTile)
        // {
        //     HandleHover();
        //     lastTileHighlightedFor = this.InputManager.HoveredTile;
        // }

        if (this.InputManager.CurrentAbility != null)
        {
            return this.InputManager.abilityState;
        }
        else if (this.InputManager.PlayerLeftClick && this.InputManager.CurrentPawn.HasMovesLeft() && this.InputManager.ClickedTile != null)
        {
            Pawn p = this.InputManager.ClickedTile.GetPawn();
            if (p != null && p == this.InputManager.CurrentPawn)
            {
                // facing without a move still counts as one
                this.InputManager.CurrentPawn.ExpendActionPoints(1);
                return this.InputManager.facingState;
            }
            else
            {
                return this.InputManager.movingState;
            }
        }

        return this;
    }

    public override void Exit()
    {
        // WeaponAbilityData weaponAbility = this.InputManager.CurrentPawn.GetBasicAttack();
        // this.InputManager.CurrentPawn.CurrentTile.HighlightTilesInRange(this.InputManager.CurrentPawn, weaponAbility, false, Tile.TileHighlightType.AttackRange);
        // this.InputManager.CurrentPawn.CurrentTile.HighlightTilesInRange(this.InputManager.CurrentPawn, this.InputManager.CurrentPawn.MoveRange, false, Tile.TileHighlightType.Move);
    }

    public override void Enter()
    {
        // idle state doesn't do much 
    }

    private void HandleHover()
    {
        Pawn hoveredPawn = this.InputManager.HoveredTile.GetPawn();
        if (hoveredPawn != null)
        {
            // we should display set facing UI
            if (hoveredPawn == this.InputManager.CurrentPawn)
            {

            }
            // we should display attack UI
            else
            {
                WeaponAbilityData weaponAbility = this.InputManager.CurrentPawn.GetBasicAttack();
                this.InputManager.CurrentPawn.CurrentTile.HighlightTilesInRange(this.InputManager.CurrentPawn, weaponAbility, true, Tile.TileHighlightType.AttackRange);
            }
        }
        else
        {
            // we should display moving Ui
            this.InputManager.CurrentPawn.CurrentTile.HighlightTilesInRange(this.InputManager.CurrentPawn, this.InputManager.CurrentPawn.MoveRange, true, Tile.TileHighlightType.Move);
        }
    }
}

public class MovingState : InputState
{
    private bool _movingDone;

    public override InputState Update()
    {
        if (_movingDone)
        {
            return this.InputManager.facingState;
        }
        else if (this.InputManager.CurrentAbility != null)
        {
            return this.InputManager.abilityState;
        }
        else if (this.InputManager.PlayerLeftClick)
        {
            HandleAttemptToMove();
        }
        else if (this.InputManager.PlayerRightClick)
        {
            return this.InputManager.idleState;
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
        this.InputManager.CurrentPawn.CurrentTile.HighlightTilesInRange(this.InputManager.CurrentPawn, this.InputManager.CurrentPawn.MoveRange, true, Tile.TileHighlightType.Move);
    }

    private void ClearMoveHighlight()
    {
        this.InputManager.CurrentPawn.CurrentTile.HighlightTilesInRange(this.InputManager.CurrentPawn, this.InputManager.CurrentPawn.MoveRange, false, Tile.TileHighlightType.Move);
    }

    private void HandleMoveComplete()
    {
        _movingDone = true;
        this.InputManager.CurrentPawn.OnMoved.RemoveListener(HandleMoveComplete);
        this.InputManager.SetSelectedTile(this.InputManager.CurrentPawn.CurrentTile);
    }

    private void HandleAttemptToMove()
    {
        Tile targetTile = this.InputManager.ClickedTile;
        if (targetTile != null && !targetTile.IsImpassable)
        {
            // should we even be raycasting and all this? Should we perhaps instead just report clicks from the Tile script? So that it just 
            // directly tells the selection manager what was clicked?
            // maybe later.
            // I don't think we need to check this btw.
            Pawn targetPawn = targetTile.GetPawn();
            if (this.InputManager.CurrentPawn.CurrentTile.GetTilesInRange(this.InputManager.CurrentPawn, this.InputManager.CurrentPawn.MoveRange).Contains(targetTile) && targetPawn == null && this.InputManager.SelectedTile != null)
            {
                if (this.InputManager.CurrentPawn.HasPathToTile(targetTile))
                {
                    ClearMoveHighlight();
                    this.InputManager.CurrentPawn.OnMoved.AddListener(HandleMoveComplete);
                    this.InputManager.CurrentPawn.TryMoveToTile(targetTile);
                }
            }
        }
    }
    
    private void HandleHover()
    {
        Pawn currentPawn = this.InputManager.CurrentPawn;
        Tile hoveredTile = this.InputManager.HoveredTile;

        if (currentPawn != null)
        {
            // we should display moving Ui
            currentPawn.CurrentTile.HighlightTilesInRange(this.InputManager.CurrentPawn, this.InputManager.CurrentPawn.MoveRange, true, Tile.TileHighlightType.Move);
        }
    }
}

public class SetFacingState : InputState
{
    public override InputState Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        HighlightAttackRange(false);

        this.InputManager.CurrentPawn.SetFacing(mouseWorldPos);

        HighlightAttackRange(true);

        if (this.InputManager.PlayerLeftClick)
        {
            return this.InputManager.abilityState;
            // BattleManager.Instance.PawnActivated(TheSelectionManager.CurrentPawn);
            // return TheSelectionManager.idleState;
        }

        return this;
    }

    public override void Exit()
    {
        HighlightAttackRange(false);
    }

    public override void Enter()
    {
        ;
    }

    private void HighlightAttackRange(bool shouldHighlight)
    {
        if (this.InputManager.CurrentPawn != null && this.InputManager.CurrentPawn.GetWeaponAbilities().Any())
        {
            WeaponAbilityData weaponAbility = this.InputManager.CurrentPawn.GetBasicAttack();
            this.InputManager.CurrentPawn.CurrentTile.HighlightTilesInRange(this.InputManager.CurrentPawn, weaponAbility, shouldHighlight, Tile.TileHighlightType.AttackRange);
        }
    }
}

// should this be removeD?
public class UsingAbilityState : InputState
{
    public override InputState Update()
    {
        // if (TheSelectionManager.PlayerLeftClick)
        // {
        //     if (HandleAttemptUseAbility())
        //     {
        return this.InputManager.inactiveState;
        //     }
        // }
        // else if (TheSelectionManager.PlayerRightClick)
        // {
        //     return TheSelectionManager.idleState;
        // }

        // return this;
    }

    public override void Exit()
    {
        ClearAttackHighlight();
    }

    public override void Enter()
    {
        HandleAttemptUseAbility();
        // TheSelectionManager.CurrentPawn.CurrentTile.HighlightTilesInRange(TheSelectionManager.CurrentPawn, Ability.SelectedAbility.range, true, Tile.TileHighlightType.AttackRange);
    }

    private void ClearAttackHighlight()
    {
        // TheSelectionManager.CurrentPawn.CurrentTile.HighlightTilesInRange(TheSelectionManager.CurrentPawn, TheSelectionManager.CurrentPawn.MoveRange, false, Tile.TileHighlightType.Move);
    }

    private bool HandleAttemptUseAbility()
    {
        Tile newTile = this.InputManager.ClickedTile;
        // if (newTile != null && !newTile.IsImpassable)
        // {
        //     Pawn targetPawn = newTile.GetPawn();
        //     if (Ability.SelectedAbility != null && targetPawn != null && TheSelectionManager.CurrentPawn.IsTargetInRange(targetPawn, Ability.SelectedAbility))
        //     {
        //         bool isAlly = targetPawn.OnPlayerTeam == TheSelectionManager.CurrentPawn.OnPlayerTeam;

        //         // only allow support ability use on allies or attack ability use on enemies
        //         // THIS NEEDS UPDATE IF WE ADD OTHER ABILITY TYPES OTHER THAN WEAPON OR SUPPORT
        //         if (Ability.SelectedAbility is SupportAbilityData && isAlly ||
        //             Ability.SelectedAbility is WeaponAbilityData && !isAlly)
        //         {


        Pawn targetPawn = this.InputManager.CurrentPawn.GetPawnInAttackRange();

        if (targetPawn != null)
        {
            Ability a = this.InputManager.CurrentPawn.GetBasicAttack();//GetWeaponAbilities()[0];
            a.Activate(this.InputManager.CurrentPawn, targetPawn);
            return true;
        }
        else
        {
            BattleManager.Instance.PawnActivated(this.InputManager.CurrentPawn);
            return false;
        }
        // }
        //     }
        // }
    }
}
