using System.Collections.Generic;
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

    // out of bounds tiles
    [SerializeField] private List<Sprite> oobTileSprites= new();
    [SerializeField] private List<Sprite> tileSprites = new();
    [SerializeField] private List<Sprite> terrainSprites = new();
    [SerializeField] private Sprite shopTileSprite;
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

    [SerializeField] private ItemUIImmersive _item;
    [SerializeField] private Pawn _pawn => _inhabitant as Pawn;
    private TileInhabitant _inhabitant;

    private bool _isSelected;
    //private bool _isInMoveRange;
    private List<Tile> _adjacentTiles = new();

    public PointNode PathfindingNode => _pathfindingNode;
    private PointNode _pathfindingNode;
    public Vector2 Coordinates => _coordinates;
    private Vector2 _coordinates;

    private Sprite _prevHighlightSprite;

    public bool OutOfBounds = true;

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

    public void SetShopTile(bool isShopTile)
    {
        terrainSpriteRend.gameObject.SetActive(isShopTile);
        
        if (isShopTile)
        {
            terrainSpriteRend.gameObject.SetActive(isShopTile);
            terrainSpriteRend.sprite = shopTileSprite;
            terrainSpriteRend.sortingLayerName = "DeadCharacters";
        }
        else
        {
            terrainSpriteRend.sprite = terrainSprites[Random.Range(0, terrainSprites.Count)];
            terrainSpriteRend.sortingLayerName = "Characters";
        }
    }

    public void SetTerrainObstacle(bool isImpassable)
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

    public void Initialize(Dictionary<Vector2, Tile> tiles, Vector2 coord, bool isImpassable)
    {
        _coordinates = coord;

        PointGraph graph = AstarPath.active.data.pointGraph;
        _pathfindingNode = graph.AddNode((Int3)transform.position);

        SetTerrainObstacle(isImpassable);

        // set up adjacent tiles
        Vector2 p = new Vector2(coord.x + 1, coord.y);
        if (tiles.ContainsKey(p))
        {
            _adjacentTiles.Add(tiles[p]);
        }

        p.x = coord.x - 1;
        if (tiles.ContainsKey(p))
        {
            _adjacentTiles.Add(tiles[p]);
        }

        p.y = coord.y + 1;
        p.x = coord.x;
        if (tiles.ContainsKey(p))
        {
            _adjacentTiles.Add(tiles[p]);
        }

        p.y = coord.y - 1;
        if (tiles.ContainsKey(p))
        {
            _adjacentTiles.Add(tiles[p]);
        }

        if (OutOfBounds)
        {
            tileSpriteRend.sprite = oobTileSprites[Random.Range(0, oobTileSprites.Count)];
        }
        else
        {
            tileSpriteRend.sprite = tileSprites[Random.Range(0, tileSprites.Count)];
        }

        terrainSpriteRend.sprite = terrainSprites[Random.Range(0, terrainSprites.Count)];
    }

    public void UpdateNodeConnections()
    {
        foreach (Tile t in _adjacentTiles)
        {
            _pathfindingNode.AddConnection(t.PathfindingNode, 1);
        }
    }

    /// <summary>
    /// Get adjacent tile that aligns to the direction given
    ///  Thanks ChatGPT :)
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public Tile GetNextTileInDirection(Vector3 direction)
    {
        direction = direction.normalized;
        
        return _adjacentTiles
            .OrderByDescending(t => Vector3.Dot((t.transform.position - transform.position).normalized, direction))
            .FirstOrDefault();
    }

    /// <summary>
    /// For getting adjacent tiles AROUND a specific tile
    /// </summary>
    /// <param name="startTile"></param>
    /// <returns></returns>
    public Tile GetAdjacentTileInDirection(Tile startTile)
    {
        List<Tile> adjTiles = GetAdjacentTiles();

        if (startTile.Coordinates.x > Coordinates.x)
        {
            // attack was down, get target to left
            Tile t = adjTiles.Where(tile => tile.Coordinates.x < Coordinates.x).FirstOrDefault();
            return t;
        }
        else if (startTile.Coordinates.x < Coordinates.x)
        {
            // attack was up, get target to right
            Tile t = adjTiles.Where(tile => tile.Coordinates.x > Coordinates.x).FirstOrDefault();
            return t;
        }
        else if (startTile.Coordinates.y > Coordinates.y)
        {
            // attack was to the right, get target down
            Tile t = adjTiles.Where(tile => tile.Coordinates.y < Coordinates.y).FirstOrDefault();
            return t;
        }
        else
        {
            // attack was to left, get target up
            Tile t = adjTiles.Where(tile => tile.Coordinates.y > Coordinates.y).FirstOrDefault();
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

        if (startTile.Coordinates.x > Coordinates.x)
        {
            // attack was to left, get target up
            Tile t = adjTiles.Where(tile => tile.Coordinates.y > Coordinates.y).FirstOrDefault();
            return t;
        }
        else if (startTile.Coordinates.x < Coordinates.x)
        {
            // attack was to the right, get target down
            Tile t = adjTiles.Where(tile => tile.Coordinates.y < Coordinates.y).FirstOrDefault();
            return t;
        }
        else if (startTile.Coordinates.y > Coordinates.y)
        {
            // attack was down, get target to left
            Tile t = adjTiles.Where(tile => tile.Coordinates.x < Coordinates.x).FirstOrDefault();
            return t;
        }
        else
        {
            // attack was up, get target to right
            Tile t = adjTiles.Where(tile => tile.Coordinates.x > Coordinates.x).FirstOrDefault();
            return t;
        }
    }

    public List<Tile> GetAdjacentTiles()
    {
        return _adjacentTiles;
    }

    public int GetTileDistance(Tile targetTile)
    {
        int yDiff = (int) Mathf.Abs(targetTile.Coordinates.y - _coordinates.y);
        int xDiff = (int) Mathf.Abs(targetTile.Coordinates.x - _coordinates.x);
        return xDiff + yDiff;
    }

    public bool IsSelectable()
    {
        return _pawn != null;
    }
    
    public void ClearInhabitant()
    {
        SetSelected(false);
        SetInhabitant(null);
    }

    public void SetInhabitant(TileInhabitant thing)
    {
        _inhabitant = thing;

        if (_inhabitant != null)
        {
            _inhabitant.CurrentTile = this;
        }
    }

    public TileInhabitant GetInhabitant()
    {
        return _inhabitant;
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
        Vector2 p = new();
        Tile t;
        // start at x - range because we want to be able to move backwards. 
        // Go to x + range because we want to be able to move forwards. Same for Y.
        for (int x = (int)Coordinates.x - pawnMoveRange; x <= pawnMoveRange + Coordinates.x; x++)
        {
            p.x = x;
            for (int y = (int)Coordinates.y - pawnMoveRange; y <= pawnMoveRange + Coordinates.y; y++)
            {
                p.y = y;
                
                // don't go outside of grid
                if  (!GridGenerator.Instance.Tiles.ContainsKey(p))
                {
                    continue;
                }

                t = GridGenerator.Instance.Tiles[p];

                if (t.IsInRangeOf(this, pawnMoveRange) && t.IsTraversableByThisPawn(_pawn))
                {
                    tilesInRange.Add(t);
                }
            }
        }
        
        return tilesInRange;
    }

    public bool IsTraversableByThisPawn(Pawn traveller)
    {
        // Does it contain pawns or impassable tiles?
        return !IsImpassable && (_pawn == null || _pawn != null && traveller.OnPlayerTeam == _pawn.OnPlayerTeam);
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
        if (Ability.SelectedAbility != null && BattleManager.Instance.CurrentPawn.CurrentTile.IsInRangeOf(this, Ability.SelectedAbility.range))
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
        if (!OutOfBounds && range > 0 && IsTraversableByThisPawn(_pawn) &&
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
                if (!t._isSelected)
                {
                    t.HighlightTilesInRangeRecursive(subjectPawn, range, isHighlighting, highlightType);
                }
            }
        }
        else
        {
            return;
        }
    }

    /// <summary>
    /// Can this pawn traverse this tile?
    /// </summary>
    /// <param name="traversingPawn"></param>
    /// <returns></returns>
    public bool CanTraverse(Pawn traversingPawn)
    {
        if (_pawn != null)
        {
            return !IsImpassable && _pawn.OnPlayerTeam == traversingPawn.OnPlayerTeam;
        }

        return !IsImpassable;
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
