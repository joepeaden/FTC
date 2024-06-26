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

    [SerializeField] private Character _character;
    private bool _isSelected;
    private bool _isRangeHighighted;
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

    public bool IsSelectable()
    {
        return _character != null;
    }

    public void CharacterEnterTile(Character newCharacter)
    {
        _character = newCharacter;
        SelectionManager.SetSelectedTile(this);
    }

    public void SetActionTile(Tile actionTile)
    {
        if (_character != null)
        {
            // hmmm. I had no idea that objects of the same type can access eachother's private variables...
            if (actionTile._isRangeHighighted)
            {
                _character.GoToSpot(actionTile.transform.position);
                SetSelected(false);
                _character = null;
            }
        }
    }

    public void ToggleSelected()
    {
        SetSelected(!_isSelected);
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

        if (_character != null)
        {
            if (_isSelected)
            {
                int charMoveRange = _character.MoveRange;
                foreach (Tile t in adjacentTiles)
                {
                    t.CheckIfInRange(charMoveRange, true);
                }
            }
            else
            {
                int charMoveRange = _character.MoveRange;
                foreach (Tile t in adjacentTiles)
                {
                    t.CheckIfInRange(charMoveRange, false);
                }
            }
        }
    }

    public void CheckIfInRange(int moveRange, bool isHighlighting)
    {
        if (moveRange > 0)
        {
            moveRange--;

            if (!_isSelected)
            {
                tileOverlayUI.enabled = isHighlighting;
                tileOverlayUI.sprite = moveRangeSprite;
                _isRangeHighighted = isHighlighting;
            }

            foreach (Tile t in adjacentTiles)
            {
                t.CheckIfInRange(moveRange, isHighlighting);
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
