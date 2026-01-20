using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SelectionManager : MonoBehaviour
{
    public Tile SelectedTile => _selectedTile;
    private Tile _selectedTile;

    private bool _playerControlsEnabled = false;

    /// <summary>
    /// If the player hasn't attempted to move or selected an action (there's
    /// no action or movement highlights at this point)
    /// </summary>
    private bool _inIdleMode = true;

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
        if (FlowDirector.Instance.CurrentPawn.actionPoints < Pawn.BASE_ACTION_POINTS && FlowDirector.Instance.CurrentPawn.HasActionsRemaining())
        {
            _selectedTile.HighlightTilesInRange(FlowDirector.Instance.CurrentPawn, 0, FlowDirector.Instance.CurrentPawn.MoveRange, true, Tile.TileHighlightType.Move);
        }
    }

    public void ClearSelectedTile()
    {
        if (_selectedTile != null)
        {
            _selectedTile.SetSelected(false);
            _selectedTile = null;
        }
    }

    public void HandleTurnChange(bool isPlayerTurn)
    {
        _playerControlsEnabled = isPlayerTurn;

        //if (_selectedTile != null)
        //{
        //    _selectedTile.SetSelected(false);
            SetSelectedTile(FlowDirector.Instance.CurrentPawn.CurrentTile);
        //}

        if (isPlayerTurn)
        {
            SetIdleMode(true);
        }
    }

    public void DisablePlayerControls()
    {
        _playerControlsEnabled = false;
        //_selectedTile = null;
    }

    public void EnablePlayerControls()
    {
        _playerControlsEnabled = true;
    }

    public void ClearHighlights()
    {
        Pawn currentPawn = FlowDirector.Instance.CurrentPawn;

        if (Ability.SelectedAbility!= null)
        {
            _selectedTile.HighlightTilesInRange(currentPawn, 0, Ability.SelectedAbility.range, false, Tile.TileHighlightType.AttackRange);
        }

        _selectedTile.HighlightTilesInRange(currentPawn, 0, currentPawn.MoveRange, false, Tile.TileHighlightType.Move);
    }

    public void SetIdleMode(bool isIdle)
    {
        Pawn currentPawn = FlowDirector.Instance.CurrentPawn;
        Ability currentAction = Ability.SelectedAbility;

        // already not in idle mode and maybe switching actions -
        // clear highlights.
        if (_inIdleMode == isIdle && _inIdleMode == false)
        {
            ClearHighlights();
        }

        _inIdleMode = isIdle;

        if (currentAction != null)
        {
            _selectedTile.HighlightTilesInRange(currentPawn, 0, Ability.SelectedAbility.range, !isIdle, Tile.TileHighlightType.AttackRange);
        }
        else
        {
            _selectedTile.HighlightTilesInRange(currentPawn, 0, currentPawn.MoveRange, !isIdle, Tile.TileHighlightType.Move);
        }
    }

    private void Update()
    {
        if (_playerControlsEnabled && Input.GetMouseButtonDown(0))
        {
            Pawn currentPawn = FlowDirector.Instance.CurrentPawn;

            Vector3 mousePos = CameraManager.MainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, -Vector3.forward);

            if (hits.Length > 0)
            {
                if (_inIdleMode)
                {
                    SetIdleMode(false);
                }
                else
                {
                    foreach (RaycastHit2D hit in hits)
                    {
                        Tile newTile = hit.transform.GetComponent<Tile>();
                        if (newTile != null && !newTile.IsImpassable)
                        {
                            if (currentPawn.OnPlayerTeam)
                            {
                                Pawn targetPawn = newTile.GetPawn();
                                if (Ability.SelectedAbility != null && targetPawn != null && currentPawn.IsTargetInRange(targetPawn, Ability.SelectedAbility))
                                {
                                    bool isAlly = targetPawn.OnPlayerTeam == currentPawn.OnPlayerTeam;

                                    // only allow support ability use on allies or attack ability use on enemies
                                    // THIS NEEDS UPDATE IF WE ADD OTHER ABILITY TYPES OTHER THAN WEAPON OR SUPPORT
                                    if (Ability.SelectedAbility is SupportAbilityData && isAlly ||
                                        Ability.SelectedAbility is WeaponAbilityData && !isAlly)
                                    {
                                        ClearHighlights();
                                        Ability.SelectedAbility.Activate(currentPawn, targetPawn);
                                    }
                                }
                                else if (currentPawn.CurrentTile.GetTilesInRange(currentPawn, 0, currentPawn.MoveRange).Contains(newTile) && targetPawn == null && Ability.SelectedAbility == null && _selectedTile != null)
                                {
                                    ClearHighlights();
                                    currentPawn.TryMoveToTile(newTile);
                                }
                            }

                        }
                    }
                }
            }
        }
        else if (_playerControlsEnabled && Input.GetMouseButtonDown(1))
        {
            SetIdleMode(true);
        }
    }
}
