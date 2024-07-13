using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public Tile SelectedTile => _selectedTile;
    private Tile _selectedTile;

    private bool playerControlsEnabled = false;

    public void SetSelectedTile(Tile newTile)
    {
        if (_selectedTile != null)
        {
            _selectedTile.SetSelected(false);
        }

        _selectedTile = newTile;
        _selectedTile.SetSelected(true);
    }

    public void HandleTurnChange(bool isPlayerTurn)
    {
        playerControlsEnabled = isPlayerTurn;

        if (_selectedTile != null)
        {
            _selectedTile.SetSelected(false);
            _selectedTile = null;
        }
    }

    public void DisablePlayerControls()
    {
        playerControlsEnabled = false;
        _selectedTile = null;
    }

    private void Update()
    {
        if (playerControlsEnabled && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            Vector3 mousePos = CameraManager.MainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, -Vector3.forward);

            foreach (RaycastHit2D hit in hits)
            {
                Tile newTile = hit.transform.GetComponent<Tile>();
                if (newTile != null)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        // don't do anything if select the same thing
                        //if (_selectedTile == newTile)
                        //{
                        //    return;
                        //}

                        // deselect previous
                        if (_selectedTile != null)
                        {
                            _selectedTile.SetSelected(false);
                            _selectedTile = null;
                        }

                        // select new
                        if (newTile.IsSelectable())
                        {
                            SetSelectedTile(newTile);
                        }
                    }
                    else if (Input.GetMouseButton(1))
                    {
                        if (_selectedTile != null && _selectedTile.GetPawn().OnPlayerTeam)
                        {
                            Pawn currentPawn = _selectedTile.GetPawn();
                            if (currentPawn != null)
                            {
                                Pawn targetPawn = newTile.GetPawn();
                                if (targetPawn != null && newTile.IsAdjacentTo(_selectedTile) && targetPawn.OnPlayerTeam != currentPawn.OnPlayerTeam)
                                {
                                    currentPawn.AttackPawn(targetPawn);
                                }
                                else if (newTile.IsInMoveRange && targetPawn == null)
                                {
                                    currentPawn.MoveToTile(newTile);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
