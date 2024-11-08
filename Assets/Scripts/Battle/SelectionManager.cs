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
            SetSelectedTile(BattleManager.Instance.CurrentPawn.CurrentTile);
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
        Pawn currentPawn = BattleManager.Instance.CurrentPawn;
        ActionData currentAction = BattleManager.Instance.CurrentAction;

        if (currentAction != null)
        {
            _selectedTile.HighlightTilesInRange(currentPawn, currentAction.range, false, Tile.TileHighlightType.AttackRange);
        }

        _selectedTile.HighlightTilesInRange(currentPawn, currentPawn.MoveRange, false, Tile.TileHighlightType.Move);
    }

    public void SetIdleMode(bool isIdle)
    {
        Pawn currentPawn = BattleManager.Instance.CurrentPawn;
        ActionData currentAction = BattleManager.Instance.CurrentAction;

        // already not in idle mode and maybe switching actions -
        // clear highlights.
        if (_inIdleMode == isIdle && _inIdleMode == false)
        {
            ClearHighlights();
        }

        _inIdleMode = isIdle;

        if (currentAction != null)
        {
            _selectedTile.HighlightTilesInRange(currentPawn, currentAction.range, !isIdle, Tile.TileHighlightType.AttackRange);
        }
        else
        {
            _selectedTile.HighlightTilesInRange(currentPawn, currentPawn.MoveRange, !isIdle, Tile.TileHighlightType.Move);
        }
    }

    private void Update()
    {
        if (_playerControlsEnabled && Input.GetMouseButtonDown(0))
        {
            Pawn currentPawn = BattleManager.Instance.CurrentPawn;
            ActionData currentAction = BattleManager.Instance.CurrentAction;

            if (_inIdleMode)
            {
                SetIdleMode(false);
            }
            else
            {
                Vector3 mousePos = CameraManager.MainCamera.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, -Vector3.forward);

                foreach (RaycastHit2D hit in hits)
                {
                    Tile newTile = hit.transform.GetComponent<Tile>();
                    if (newTile != null && !newTile.IsImpassable)
                    {
                        if (currentPawn.OnPlayerTeam)
                        {
                            Pawn targetPawn = newTile.GetPawn();
                            if (currentAction != null && targetPawn != null && currentPawn.IsTargetInRange(targetPawn, currentAction) && targetPawn.OnPlayerTeam != currentPawn.OnPlayerTeam)
                            {
                                ClearHighlights();
                                currentPawn.AttackPawnIfResourcesAvailable(targetPawn);
                            }
                            else if (currentPawn.CurrentTile.GetTilesInMoveRange().Contains(newTile) && targetPawn == null && currentAction == null && _selectedTile != null)
                            {
                                ClearHighlights();
                                currentPawn.TryMoveToTile(newTile);
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
