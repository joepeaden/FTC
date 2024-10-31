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

    [SerializeField] GameObject[] tilePrefabs;
    [SerializeField] int gridHeight;
    [SerializeField] int gridWidth;
    [SerializeField] float tileSize;
    [SerializeField] float impassableChancePerTile;

    private Dictionary<Point, Tile> _tiles = new();

    public List<Tile> PlayerSpawns => _playerSpawns;
    public List<Tile> EnemySpawns => _enemySpawns;
    private List<Tile> _playerSpawns = new();
    private List<Tile> _enemySpawns = new();

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

                    GameObject tilePrefab = tilePrefabs[Random.Range(0, tilePrefabs.Length)];
                    GameObject tileGO = Instantiate(tilePrefab, transform);

                    float posX = (x * tileSize + y * tileSize) / 2f;
                    float posY = (x * tileSize - y * tileSize) / 4f;

                    tileGO.transform.position = new Vector3(posX, posY);
                    tileGO.name = "Tile (" + x + ", " + y + ")";

                    _tiles[gridPoint] = tileGO.GetComponent<Tile>();

                    if (y <= 7)
                    {
                        if (x == 0)
                        {
                            _playerSpawns.Add(tileGO.GetComponent<Tile>());
                        }
                        else if (x == 7)
                        {
                            _enemySpawns.Add(tileGO.GetComponent<Tile>());
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
