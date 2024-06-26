using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    private static Tile _selectedTile;

    public static void SetSelectedTile(Tile newTile)
    {
        if (_selectedTile != null)
        {
            _selectedTile.SetSelected(false);
        }

        _selectedTile = newTile;
        _selectedTile.SetSelected(true);
    }

    private void Update()
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
                    if (_selectedTile == newTile)
                    {
                        return;
                    }

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
                    if (_selectedTile != null)
                    {
                        _selectedTile.SetActionTile(newTile);
                    }
                }
            }
        }
    }
}
