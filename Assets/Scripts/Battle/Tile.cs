using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Pathfinding;
using UnityEngine;
using UnityEngine.Events;

public class Tile : MonoBehaviour
{
    public enum TileHighlightType
    {
        Move,
        AttackRange,
        AttackTarget
    }

    public const int BASE_AP_TO_TRAVERSE = 2;

    public static UnityEvent<Tile> OnTileHoverStart = new();
    public static UnityEvent<Tile> OnTileHoverEnd = new();
    private static UnityEvent OnTileSelectChange = new();

    [SerializeField] private List<Sprite> tileSprites = new();
    [SerializeField] private List<Sprite> terrainSprites = new();
    [SerializeField] private SpriteRenderer tileSpriteRend;
    [SerializeField] private SpriteRenderer terrainSpriteRend;
    [SerializeField] private Sprite selectionSprite;
    //[SerializeField] private Sprite hoverSprite;
    [SerializeField] private Sprite moveRangeSprite;
    [SerializeField] private Sprite attackHighlightSprite;
    [SerializeField] private Sprite attackTargetHighlightSprite;
    [SerializeField] private SpriteRenderer tileOverlayUI;
    [SerializeField] private SpriteRenderer tileHoverUI;

    public bool IsImpassable => _isImpassable;
    private bool _isImpassable = false;

    [SerializeField] private Pawn _pawn;
    private bool _isSelected;
    //private bool _isInMoveRange;
    private List<Tile> _adjacentTiles = new();

    public PointNode PathfindingNode => _pathfindingNode;
    private PointNode _pathfindingNode;
    public Point Coordinates => _coordinates;
    private Point _coordinates;

    private Sprite _prevHighlightSprite;

    private void Awake()
    {
        OnTileSelectChange.AddListener(ResetTileVisuals);
    }

    private void OnDestroy()
    {
        OnTileSelectChange.RemoveListener(ResetTileVisuals);
    }

    public int GetTerrainSortingOrder()
    {
        return terrainSpriteRend.sortingOrder;
    }
    
    public void SetTerrainSortingOrder(int level)
    {
        terrainSpriteRend.sortingOrder = level;
        tileSpriteRend.sortingOrder = level;
    }

    public bool IsInRangeOf(Tile t, int range)
    {
        return GetTileDistance(t) <= range;
    }

    private void ResetTileVisuals()
    {
        tileOverlayUI.enabled = false;
    }

    public void SetImpassable(bool isImpassable)
    {
        if (_isImpassable == isImpassable)
        {
            return;
        }

        _isImpassable = isImpassable;

        if (isImpassable)
        {
            terrainSpriteRend.gameObject.SetActive(true);
        }
        else
        {
            if (terrainSpriteRend.gameObject.activeInHierarchy)
            {
                terrainSpriteRend.gameObject.SetActive(false);
            }
        }

        _pathfindingNode.Walkable = !IsImpassable;
    }

    public void Initialize(Dictionary<Point, Tile> tiles, Point coord, bool isImpassable)
    {
        _coordinates = coord;

        PointGraph graph = AstarPath.active.data.pointGraph;
        _pathfindingNode = graph.AddNode((Int3)transform.position);

        SetImpassable(isImpassable);

        // set up adjacent tiles
        Point p = new Point(coord.X + 1, coord.Y);
        if (tiles.ContainsKey(p))
        {
            _adjacentTiles.Add(tiles[p]);
        }

        p.X = coord.X - 1;
        if (tiles.ContainsKey(p))
        {
            _adjacentTiles.Add(tiles[p]);
        }

        p.Y = coord.Y + 1;
        p.X = coord.X;
        if (tiles.ContainsKey(p))
        {
            _adjacentTiles.Add(tiles[p]);
        }

        p.Y = coord.Y - 1;
        if (tiles.ContainsKey(p))
        {
            _adjacentTiles.Add(tiles[p]);
        }

        tileSpriteRend.sprite = tileSprites[Random.Range(0, tileSprites.Count)];
        terrainSpriteRend.sprite = terrainSprites[Random.Range(0, terrainSprites.Count)];
    }

    public void UpdateNodeConnections()
    {
        foreach (Tile t in _adjacentTiles)
        {
            _pathfindingNode.AddConnection(t.PathfindingNode, 1);
        }
    }

    public Tile GetAdjacentTileInDirection(Tile startTile)
    {
        List<Tile> adjTiles = GetAdjacentTiles();

        if (startTile.Coordinates.X > Coordinates.X)
        {
            // attack was down, get target to left
            Tile t = adjTiles.Where(tile => tile.Coordinates.X < Coordinates.X).FirstOrDefault();
            return t;
        }
        else if (startTile.Coordinates.X < Coordinates.X)
        {
            // attack was up, get target to right
            Tile t = adjTiles.Where(tile => tile.Coordinates.X > Coordinates.X).FirstOrDefault();
            return t;
        }
        else if (startTile.Coordinates.Y > Coordinates.Y)
        {
            // attack was to the right, get target down
            Tile t = adjTiles.Where(tile => tile.Coordinates.Y < Coordinates.Y).FirstOrDefault();
            return t;
        }
        else
        {
            // attack was to left, get target up
            Tile t = adjTiles.Where(tile => tile.Coordinates.Y > Coordinates.Y).FirstOrDefault();
            return t;
        }
    }

    /// <summary>
    /// Get the next tile clockwise from the start tile that is still adjacent
    /// to this one
    /// </summary>
    /// <param name="startTile"></param>
    /// <returns></returns>
    public Tile GetClockwiseNextTile(Tile startTile)
    {
        List<Tile> adjTiles = GetAdjacentTiles();

        if (startTile.Coordinates.X > Coordinates.X)
        {
            // attack was to left, get target up
            Tile t = adjTiles.Where(tile => tile.Coordinates.Y > Coordinates.Y).FirstOrDefault();
            return t;
        }
        else if (startTile.Coordinates.X < Coordinates.X)
        {
            // attack was to the right, get target down
            Tile t = adjTiles.Where(tile => tile.Coordinates.Y < Coordinates.Y).FirstOrDefault();
            return t;
        }
        else if (startTile.Coordinates.Y > Coordinates.Y)
        {
            // attack was down, get target to left
            Tile t = adjTiles.Where(tile => tile.Coordinates.X < Coordinates.X).FirstOrDefault();
            return t;
        }
        else
        {
            // attack was up, get target to right
            Tile t = adjTiles.Where(tile => tile.Coordinates.X > Coordinates.X).FirstOrDefault();
            return t;
        }
    }

    public List<Tile> GetAdjacentTiles()
    {
        return _adjacentTiles;
    }

    public int GetTileDistance(Tile targetTile)
    {
        int yDiff = Mathf.Abs(targetTile.Coordinates.Y - _coordinates.Y);
        int xDiff = Mathf.Abs(targetTile.Coordinates.X - _coordinates.X);
        return xDiff + yDiff;
    }

    public bool IsSelectable()
    {
        return _pawn != null;
    }

    public void PawnEnterTile(Pawn newPawn)
    {
        _pawn = newPawn;
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
        return _adjacentTiles.Contains(t);
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
        foreach (Tile t in _adjacentTiles)
        {
            t.GetTilesInMoveRangeRecursive(_pawn, pawnMoveRange, tilesInRange);
        }
        
        return tilesInRange;
    }

    public bool IsTraversableByThisPawn(Pawn traveller)
    {
        // Does it contain pawns or impassable tiles?
        return !IsImpassable && (_pawn == null || _pawn != null && traveller.OnPlayerTeam == _pawn.OnPlayerTeam);
    }

    private List<Tile> GetTilesInMoveRangeRecursive(Pawn traveller, int pawnMoveRange, List<Tile> tilesInRange)
    {
        if (pawnMoveRange > 0 && IsTraversableByThisPawn(traveller))
        {
            pawnMoveRange--;

            if (!_isSelected)
            {
                tilesInRange.Add(this);
            }

            foreach (Tile t in _adjacentTiles)
            {
                if (!tilesInRange.Contains(t))
                {
                    t.GetTilesInMoveRangeRecursive(traveller, pawnMoveRange, tilesInRange);
                }
            }
        }

        return tilesInRange;
    }

    public void HighlightTileAsActive()
    {
        tileOverlayUI.enabled = true;
        tileOverlayUI.sprite = selectionSprite;
    }

    public void SetSelected(bool isSelected)
    {
        _isSelected = isSelected;

        if (_isSelected)
        {
            OnTileSelectChange.Invoke();
            HighlightTileAsActive();
        }
        else
        {
            tileOverlayUI.enabled = false;
        }
    }

    public void HighlightForAction()
    {
        tileOverlayUI.enabled = true;
        tileOverlayUI.sprite = attackTargetHighlightSprite;
    }

    public void ClearActionHighlight()
    {
        if (BattleManager.Instance.CurrentAction != null && BattleManager.Instance.CurrentPawn.CurrentTile.IsInRangeOf(this, BattleManager.Instance.CurrentAction.range))
        {
            tileOverlayUI.sprite = attackHighlightSprite;
        }
        else if (BattleManager.Instance.CurrentPawn.HasMovesLeft() && BattleManager.Instance.CurrentPawn.CurrentTile.IsInRangeOf(this, BattleManager.Instance.CurrentPawn.MoveRange))
        {
            tileOverlayUI.sprite = moveRangeSprite;
        }
        else
        {
            tileOverlayUI.enabled = false;
        }
    }

    public void HighlightTilesInRange(Pawn subjectPawn, int range, bool isHighlighting, TileHighlightType highlightType)
    {
        if (!_isSelected)
        {
            tileOverlayUI.enabled = isHighlighting;

            switch (highlightType)
            {
                case TileHighlightType.Move:
                    tileOverlayUI.sprite = moveRangeSprite;
                    break;
                case TileHighlightType.AttackRange:
                    tileOverlayUI.sprite = attackHighlightSprite;
                    break;
            }
        }

        foreach (Tile t in _adjacentTiles)
        {
            t.HighlightTilesInRangeRecursive(subjectPawn, range, isHighlighting, highlightType);
        }
    }

    public void HighlightTilesInRangeRecursive(Pawn subjectPawn, int range, bool isHighlighting, TileHighlightType highlightType)
    {
        if (range > 0 && !IsImpassable &&
            (_pawn == null ||
            (subjectPawn.OnPlayerTeam == _pawn.OnPlayerTeam && TileHighlightType.Move == highlightType ||
            TileHighlightType.Move != highlightType)))
        {
            range--;

            if (!_isSelected)
            {
                tileOverlayUI.enabled = isHighlighting;

                switch (highlightType)
                {
                    case TileHighlightType.Move:
                        tileOverlayUI.sprite = moveRangeSprite;
                        break;
                    case TileHighlightType.AttackRange:
                        tileOverlayUI.sprite = attackHighlightSprite;
                        break;
                }
            }

            foreach (Tile t in _adjacentTiles)
            {
                t.HighlightTilesInRangeRecursive(subjectPawn, range, isHighlighting, highlightType);
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
        OnTileHoverStart.Invoke(this);
    }

    public void OnMouseExit()
    {
        tileHoverUI.enabled = false;
        OnTileHoverEnd.Invoke(this);
    }

}
