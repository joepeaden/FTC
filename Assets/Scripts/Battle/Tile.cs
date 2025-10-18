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
        AttackTarget,
        ExtendedMove
    }

    public const int BASE_AP_TO_TRAVERSE = 2;

    public UnityEvent<Tile> OnTileHoverStart = new();
    public UnityEvent<Tile> OnTileHoverEnd = new();
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
        SetTileHighlight(false);
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

    public void SetNodeTag(bool isOnPlayerTeam)
    {
        _pathfindingNode.Tag = isOnPlayerTeam ? AIPathCustom.PLAYER_TEAM_NODE_TAG : AIPathCustom.ENEMY_TEAM_NODE_TAG;
    }

    public void ClearNodeTag()
    {
        _pathfindingNode.Tag = AIPathCustom.DEFAULT_NODE_TAG;
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
        SetNodeTag(newPawn.OnPlayerTeam);
        _pawn = newPawn;
    }

    public void PawnExitTile()
    {
        ClearNodeTag();
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

    public bool IsTraversableByThisPawn(Pawn traveller)
    {
        bool canTraverse = true;
        if (traveller != null && traveller.CurrentTile != this)
        {
            int pawnTraversableTagsBitmask = traveller.GetPathfinderTraversableTagsBitmask();
            canTraverse = canTraverse && ((pawnTraversableTagsBitmask & (1 << (int)_pathfindingNode.Tag)) != 0);
        }

        // Does it contain pawns or impassable tiles?
        return !IsImpassable && canTraverse;
    }

    public void SetTileHighlight(bool enabled, Sprite sprite = null)
    {
        tileOverlayUI.enabled = enabled;

        if (sprite != null)
        {
            tileOverlayUI.sprite = sprite;
        }
    }

    public void SetSelected(bool isSelected)
    {
        _isSelected = isSelected;

        if (_isSelected)
        {
            OnTileSelectChange.Invoke();
            SetTileHighlight(true, selectionSprite);
        }
        else
        {
            SetTileHighlight(false);
        }
    }

    /// <summary>
    /// Highlights tiles within a specific range using the specified sprite.
    /// </summary>
    public void HighlightTilesInMoveRange(Pawn subjectPawn, bool isHighlighting, int minRange, int maxRange, Sprite sprite)
    {
        HashSet<Tile> tilesInRange = GetTilesInRange(subjectPawn, minRange, maxRange);

        foreach (Tile t in tilesInRange)
        {
            if (!t._isSelected)
            {
                t.SetTileHighlight(isHighlighting, sprite);
            }
        }
    }

    /// <summary>
    /// Highlights tiles within a specific range using a predefined highlight type.
    /// This method handles sprite selection internally based on the highlight type.
    /// </summary>
    public void HighlightTilesInRange(Pawn subjectPawn, bool isHighlighting, int minRange, int maxRange, TileHighlightType highlightType)
    {
        HashSet<Tile> tilesInRange = GetTilesInRange(subjectPawn, minRange, maxRange);

        Sprite spriteToUse = highlightType switch
        {
            TileHighlightType.Move => moveRangeSprite,
            TileHighlightType.AttackRange => attackHighlightSprite,
            TileHighlightType.AttackTarget => attackTargetHighlightSprite,
            TileHighlightType.ExtendedMove => attackHighlightSprite, // Using attackHighlightSprite for extended move range
            _ => moveRangeSprite
        };

        foreach (Tile t in tilesInRange)
        {
            if (!t._isSelected)
            {
                t.SetTileHighlight(isHighlighting, spriteToUse);
            }
        }
    }

    /// <summary>
    /// Legacy overload for backward compatibility.
    /// </summary>
    public void HighlightTilesInMoveRange(Pawn subjectPawn, bool isHighlighting)
    {
        HighlightTilesInMoveRange(subjectPawn, isHighlighting, 0, subjectPawn.MoveRange, moveRangeSprite);
    }

    /// <summary>
    /// Highlights tiles in range using an attack pattern from a WeaponAbilityData.
    /// This method applies the attack pattern to filter valid attack tiles based on facing direction.
    /// </summary>
    public void HighlightTilesInRange(Pawn subjectPawn, WeaponAbilityData weaponAbility, bool isHighlighting, TileHighlightType highlightType)
    {
        HashSet<Tile> validTiles = GetTilesInRange(subjectPawn, weaponAbility);
        
        // // Apply attack pattern filtering
        // AttackPattern pattern = weaponAbility.GetAttackPattern();
        // HashSet<Tile> validTiles = pattern.FilterTilesByPattern(this, subjectPawn.CurrentFacing, tilesInRange, weaponAbility.range);

        Sprite spriteToUse = highlightType switch
        {
            TileHighlightType.Move => moveRangeSprite,
            TileHighlightType.AttackRange => attackHighlightSprite,
            _ => moveRangeSprite
        };

        foreach (Tile t in validTiles)
        {
            if (!t._isSelected) 
            {
                t.SetTileHighlight(isHighlighting, spriteToUse);
            }
        }
    }

    /// <summary>
    /// Gets tiles within a specific range band (minRange to maxRange) that are actually reachable by the pawn.
    /// This method respects obstacles and only includes tiles that can actually be reached.
    /// </summary>
    public HashSet<Tile> GetTilesInRange(Pawn subjectPawn, int minRange, int maxRange)
    {
        var tilesInRange = new HashSet<Tile>();
        var queue = new Queue<(Tile tile, int distance)>();
        var visited = new HashSet<Tile>();

        // Start from the pawn's current tile
        queue.Enqueue((this, 0));
        visited.Add(this);

        while (queue.Count > 0)
        {
            var (current, dist) = queue.Dequeue();

            // Only add tiles within the specified range that are traversable
            if (dist >= minRange && dist <= maxRange && current.IsTraversableByThisPawn(subjectPawn))
            {
                tilesInRange.Add(current);
            }

            // Only explore adjacent tiles if we're still within range and the current tile is traversable
            if (dist < maxRange && current.IsTraversableByThisPawn(subjectPawn))
            {
                foreach (Tile neighbor in current._adjacentTiles)
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue((neighbor, dist + 1));
                    }
                }
            }
        }

        return tilesInRange;
    }

    /// <summary>
    /// Legacy overload for backward compatibility - gets tiles from 0 to range.
    /// </summary>
    public HashSet<Tile> GetTilesInRange(Pawn subjectPawn, int range)
    {
        return GetTilesInRange(subjectPawn, 0, range);
    }

    /// <summary>
    /// Gets tiles in range filtered by an attack pattern.
    /// Use this when you need to determine valid attack targets based on facing direction.
    /// </summary>
    public HashSet<Tile> GetTilesInRange(Pawn subjectPawn, WeaponAbilityData weaponAbility)
    {
        HashSet<Tile> tilesInRange = GetTilesInRange(subjectPawn, weaponAbility.range);
        AttackPattern pattern = weaponAbility.GetAttackPattern();
        return pattern.FilterTilesByPattern(this, subjectPawn.CurrentFacing, tilesInRange, weaponAbility.range);
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
        // tileHoverUI.enabled = true;
        OnTileHoverStart.Invoke(this);
    }

    public void OnMouseExit()
    {
        // tileHoverUI.enabled = false;
        OnTileHoverEnd.Invoke(this);
    }

}
