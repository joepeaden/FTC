using System.Collections;
using System.Collections.Generic;
using System.Drawing;
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

    void GenerateGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GameObject tilePrefab = tilePrefabs[Random.Range(0, tilePrefabs.Length)];
                GameObject tileGO = Instantiate(tilePrefab, transform);

                float posX = (x * tileSize + y * tileSize) / 2f;
                float posY = (x * tileSize - y * tileSize) / 4f;

                tileGO.transform.position = new Vector3(posX, posY);
                tileGO.name = "Tile (" + x + ", " + y + ")";

                _tiles[new Point(x, y)] = tileGO.GetComponent<Tile>();

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

        foreach (KeyValuePair<Point, Tile> kvp in _tiles)
        {
            Tile tile = kvp.Value;
            Point coord = kvp.Key;

            
            bool isImpassible = false;
            if (!_playerSpawns.Contains(tile) && !_enemySpawns.Contains(tile))
            {
                isImpassible = Random.Range(0f, 1f) < impassableChancePerTile;
            }

            tile.Initialize(_tiles, coord, isImpassible);

            if (!CheckConnectivity(_playerSpawns[0]))
            {
                EnsureConnectivity();//tile.SetImpassable(false);
            }
        }

        //if (!CheckConnectivity(_playerSpawns[0]))
        //{
        //    EnsureConnectivity();
        //}
    }

    // BFS Connectivity Check
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

        // Try turning groups of impassable tiles to passable in sequence
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
