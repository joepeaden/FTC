using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Sprite selectionSprite;
    [SerializeField] private Sprite hoverSprite;
    [SerializeField] private Sprite moveRangeSprite;
    [SerializeField] private SpriteRenderer tileOverlayUI;
    [SerializeField] private SpriteRenderer tileHoverUI;

    [SerializeField] private Pawn _pawn;
    private bool _isSelected;
    public bool IsInMoveRange => _isInMoveRange;
    private bool _isInMoveRange;
    private List<Tile> adjacentTiles = new();

    public void Initialize(Dictionary<Point, Tile> tiles, Point coord)
    {
        Point p = new Point(coord.X + 1, coord.Y);
        if (tiles.ContainsKey(p))
        {
            adjacentTiles.Add(tiles[p]);
        }

        p.X = coord.X - 1;
        if (tiles.ContainsKey(p))
        {
            adjacentTiles.Add(tiles[p]);
        }

        p.Y = coord.Y + 1;
        p.X = coord.X;
        if (tiles.ContainsKey(p))
        {
            adjacentTiles.Add(tiles[p]);
        }

        p.Y = coord.Y - 1;
        if (tiles.ContainsKey(p))
        {
            adjacentTiles.Add(tiles[p]);
        }
    }

    public List<Tile> GetAdjacentTiles()
    {
        return adjacentTiles;
    }

    public bool IsSelectable()
    {
        return _pawn != null;
    }

    public void PawnEnterTile(Pawn newPawn)
    {
        _pawn = newPawn;

        if (_pawn.OnPlayerTeam)
        {
            //SelectionManager.SetSelectedTile(this);
        }
    }

    public void PawnExitTile()
    {
        SetSelected(false);
        _pawn = null;
    }

    public Pawn GetPawn()
    {
        return _pawn;
    }

    public bool IsAdjacentTo(Tile t)
    {
        return adjacentTiles.Contains(t);
    }

    public void ToggleSelected()
    {
        SetSelected(!_isSelected);
    }

    public List<Tile> GetTilesInMoveRange()
    {
        if (_pawn == null)
        {
            return null;
        }

        int pawnMoveRange = _pawn.MoveRange;
        List<Tile> tilesInRange = new();
        foreach (Tile t in adjacentTiles)
        {
            t.GetTilesInMoveRangeRecursive(pawnMoveRange, tilesInRange);
        }
        
        return tilesInRange;
    }

    private List<Tile> GetTilesInMoveRangeRecursive(int pawnMoveRange, List<Tile> tilesInRange)
    {
        if (pawnMoveRange > 0)
        {
            tilesInRange.Add(this);
            pawnMoveRange--;

            if (!_isSelected)
            {
                tilesInRange.Add(this);
            }

            foreach (Tile t in adjacentTiles)
            {
                if (!tilesInRange.Contains(t))
                {
                    t.GetTilesInMoveRangeRecursive(pawnMoveRange, tilesInRange);
                }
            }
        }

        return tilesInRange;
    }

    public void SetSelected(bool isSelected)
    {
        _isSelected = isSelected;

        if (_isSelected)
        {
            tileOverlayUI.enabled = true;
            tileOverlayUI.sprite = selectionSprite;
        }
        else
        {
            tileOverlayUI.enabled = false;
        }

        if (_pawn != null)
        {
            if (_isSelected)
            {
                int charMoveRange = _pawn.MoveRange;
                foreach (Tile t in adjacentTiles)
                {
                    t.ColorTilesInMoveRange(charMoveRange, true);
                }
            }
            else
            {
                int charMoveRange = _pawn.MoveRange;
                foreach (Tile t in adjacentTiles)
                {
                    t.ColorTilesInMoveRange(charMoveRange, false);
                }
            }
        }
    }

    public void ColorTilesInMoveRange(int moveRange, bool isHighlighting)
    {
        if (moveRange > 0)
        {
            moveRange--;

            if (!_isSelected)
            {
                tileOverlayUI.enabled = isHighlighting;
                tileOverlayUI.sprite = moveRangeSprite;
                _isInMoveRange = isHighlighting;
            }

            foreach (Tile t in adjacentTiles)
            {
                t.ColorTilesInMoveRange(moveRange, isHighlighting);
            }
        }
        else
        {
            return;
        }
    }


    public void OnMouseEnter()
    {
        tileHoverUI.enabled = true;
    }

    public void OnMouseExit()
    {
        tileHoverUI.enabled = false;
    }

}
