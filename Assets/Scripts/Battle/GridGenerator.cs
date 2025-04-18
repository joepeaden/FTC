using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Pathfinding;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public static GridGenerator Instance => _instance;
    private static GridGenerator _instance;

    //[SerializeField] AstarPath pathfindingGrid;

    [SerializeField] GameObject tilePrefab;
    [SerializeField] int gridHeight;
    [SerializeField] int gridWidth;
    [SerializeField] float tileSize;
    [SerializeField] float impassableChancePerTile;

    public Dictionary<Point, Tile> Tiles => _tiles;
    private Dictionary<Point, Tile> _tiles = new();

    public List<Tile> PlayerSpawns => _playerSpawns;
    public List<Tile> EnemySpawns => _enemySpawns;
    private List<Tile> _playerSpawns = new();
    private List<Tile> _enemySpawns = new();

    //private int layerIncrement = 10;

    void Awake()
    {
        _instance = this;

        GenerateGrid();

        AstarPath.active.Scan();
    }

    public void GenerateGrid()
    {
        // note for later: I really really don't need to regenerate the tiles
        // and pathfinding grid every time. Really I just need to randomize
        // terrain and update grid nodes w/ new walkability etc.

        // need to clear all nodes to set up new ones
        if (AstarPath.active.data.pointGraph.nodes != null)
        {
            AstarPath.active.data.pointGraph.nodes = null;
            AstarPath.active.Scan();
        }

        _playerSpawns.Clear();
        _enemySpawns.Clear();

        // need pathfinding graph updates to happen in this callback
        // I guess (graph updates mostly in Tile class like adding nodes)
        AstarPath.active.AddWorkItem(new AstarWorkItem(ctx =>
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Point gridPoint = new Point(x, y);
                    if (_tiles.ContainsKey(gridPoint) && _tiles[gridPoint] != null)
                    {
                        DestroyImmediate(_tiles[gridPoint].gameObject);
                    }

                    GameObject tileGO = Instantiate(tilePrefab, transform);
                    Tile tileScript = tileGO.GetComponent<Tile>();

                    float posX = (x * tileSize + y * tileSize) / 2f;
                    float posY = (x * tileSize - y * tileSize) / 4f;

                    tileGO.transform.position = new Vector3(posX, posY);
                    tileGO.name = "Tile (" + x + ", " + y + ")";

                    // 10 unit increments (the y pos are all .5 different) so that there's enough
                    // room for pawn sprite detail layering between obstacles
                    tileScript.SetTerrainSortingOrder((int)(-1 * posY * 20));

                    _tiles[gridPoint] = tileScript;

                    if (y <= 9)
                    {
                        if (x == 0)
                        {
                            _playerSpawns.Add(tileScript);
                        }
                        else if (x == 9)
                        {
                            _enemySpawns.Add(tileScript);
                        }
                    }
                }
            }

            List<Tile> randomizedListOfTiles = new();
            foreach (KeyValuePair<Point, Tile> kvp in _tiles)
            {
                Tile tile = kvp.Value;
                Point coord = kvp.Key;

                tile.Initialize(_tiles, coord, false);

                randomizedListOfTiles.Add(tile);
            }

            for (int i = 0; i < randomizedListOfTiles.Count; i++)
            {
                Tile tile = randomizedListOfTiles[Random.Range(0, randomizedListOfTiles.Count)];

                bool isImpassible = false;
                if (!_playerSpawns.Contains(tile) && !_enemySpawns.Contains(tile))
                {
                    isImpassible = Random.Range(0f, 1f) < impassableChancePerTile;
                }

                tile.SetImpassable(isImpassible);

                if (isImpassible && !CheckConnectivity(_playerSpawns[0]) || !CheckConnectivity(_enemySpawns[0]))
                {
                    tile.SetImpassable(false);
                    //EnsureConnectivity();
                }

                randomizedListOfTiles.Remove(tile);
            }

            foreach (Tile t in _tiles.Values)
            {
                t.UpdateNodeConnections();
            }

        }));

    }

    /// <summary>
    /// Get a tile by its position. Accounts for small variances in
    /// the floating point numbers in Vector3 by just getting the closest one.
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public Tile GetClosestTileToPosition(Vector3 pos)
    {
        foreach (Tile t in _tiles.Values)
        {
            if (Mathf.Abs(t.transform.position.x - pos.x) < .1f &&
                Mathf.Abs(t.transform.position.y - pos.y) < .1f &&
                Mathf.Abs(t.transform.position.z - pos.z) < .1f)
            {
                return t;
            }
        }

        return null;
    }

    /// <summary>
    /// BFS Connectivity Check
    /// </summary>
    bool CheckConnectivity(Tile startTile)
    {
        HashSet<Tile> visited = new HashSet<Tile>();
        Queue<Tile> queue = new Queue<Tile>();
        queue.Enqueue(startTile);
        visited.Add(startTile);

        while (queue.Count > 0)
        {
            Tile currentTile = queue.Dequeue(); 
            foreach (Tile neighbor in currentTile.GetAdjacentTiles())
            {
                if (!neighbor.IsImpassable && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        // Check if all non-impassable tiles are visited
        foreach (var tile in _tiles.Values)
        {
            if (!tile.IsImpassable && !visited.Contains(tile))
            {
                return false; // Not all passable tiles are reachable
            }
        }
        return true;
    }

    // Adjust impassable tiles if necessary
    void EnsureConnectivity()
    {
        List<Tile> impassableTiles = new List<Tile>();

        // Collect all impassable tiles
        foreach (var tile in _tiles.Values)
        {
            if (tile.IsImpassable)
            {
                impassableTiles.Add(tile);
            }
        }

        for (int i = 0; i < impassableTiles.Count; i++)
        {
            impassableTiles[i].SetImpassable(false); // Temporarily make passable

            // Check connectivity after clearing this tile
            if (CheckConnectivity(_playerSpawns[0]))
            {
                break; // Exit if the connectivity is restored
            }
            else
            {
                impassableTiles[i].SetImpassable(true); // Revert if no path is found
            }
        }
    }
}
