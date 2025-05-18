using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Pathfinding;
using Unity.VisualScripting;
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

    public Dictionary<Vector2, Tile> Tiles = new();
    public List<Vector2> TilesPoints = new();
    public List<Tile> TilesList = new();

    public List<Tile> TownFollowerSpawns = new();
    public List<Tile> TownRecruitSpawns = new();
    public List<Tile> TownInventorySpawns = new();
    public List<Tile> TownShopSpawns = new();
    public Tile TownMissionBoardSpawn = new();

    public List<Tile> PlayerSpawns = new();
    public List<Tile> EnemySpawns = new();

    void Awake()
    {
        _instance = this;

        SetSpawnPoints();

        // set up dictionary for easy access
        foreach (Vector2 p in TilesPoints)
        {
            Tiles[p] = TilesList[TilesPoints.IndexOf(p)];
        }

        // Generate the pathfinding nodes
        GeneratePathfinding();

        // Generate obstacles. Idk why I called them impassables.
        // GenerateImpassables();
    }

    private void SetSpawnPoints()
    {        
        TownRecruitSpawns.Clear();
        TownInventorySpawns.Clear();
        TownFollowerSpawns.Clear();
        TownShopSpawns.Clear();
        EnemySpawns.Clear();
        PlayerSpawns.Clear();

        // add town follower spawn points
        for (int x = 6; x <= 8; x++)
        {
            for (int y = 14; y <= 16; y++)
            {
                TownFollowerSpawns.Add(TilesList[TilesPoints.IndexOf(new Vector2(x, y))]);
            }
        }

        // add town recruit spawn points
        for (int x = 13; x <= 15; x++)
        {
            for (int y = 7; y <= 9; y++)
            {
                TownRecruitSpawns.Add(TilesList[TilesPoints.IndexOf(new Vector2(x, y))]);
            }
        }

        // add town inventory spawns
        for (int x = 13; x <= 15; x++)
        {
            for (int y = 14; y <= 16; y++)
            {
                TownInventorySpawns.Add(TilesList[TilesPoints.IndexOf(new Vector2(x, y))]);
            }
        }

        // add town shop spawns
        for (int x = 6; x <= 8; x++)
        {
            for (int y = 7; y <= 9; y++)
            {
                TownShopSpawns.Add(TilesList[TilesPoints.IndexOf(new Vector2(x, y))]);
            }
        }

        for (int y = 7; y < 16; y++)
        {
            PlayerSpawns.Add(TilesList[TilesPoints.IndexOf(new Vector2(6, y))]);
            EnemySpawns.Add(TilesList[TilesPoints.IndexOf(new Vector2(15, y))]);
        }
            
        TownMissionBoardSpawn = TilesList[TilesPoints.IndexOf(new Vector2(11,11))];

    }

    /// <summary>
    /// Clear the pathfinding graph and set up node connections, then scan.
    /// </summary>
    private void GeneratePathfinding()
    {
        // need to clear all nodes to set up new ones
        if (AstarPath.active.data.pointGraph.nodes != null)
        {
            AstarPath.active.data.pointGraph.nodes = null;
            AstarPath.active.Scan();
        }

        // need pathfinding graph updates to happen in this callback
        // Graph updates mostly in Tile class like adding nodes
        AstarPath.active.AddWorkItem(new AstarWorkItem(ctx =>
        {
            foreach (KeyValuePair<Vector2, Tile> kvp in Tiles)
            {
                Tile tile = kvp.Value;
                Vector2 coord = kvp.Key;

                // set up adjacency, node connections, etc.
                tile.Initialize(Tiles, coord, false);
            }

            // This second iteration is necessary. Can't do it in the same loop.
            // Tile.Initialize sets the pathfinding node, and all the pathfinding nodes need
            // to be set before Tile.UpdateNodeConnections can connect them all.
            foreach (Tile t in Tiles.Values)
            {
                t.UpdateNodeConnections();
            }
        }));

        AstarPath.active.Scan();
    }

    public void GenerateImpassables()
    {
        List<Tile> randomizedListOfTiles = new();
        foreach (KeyValuePair<Vector2, Tile> kvp in Tiles)
        {
            Tile tile = kvp.Value;
            randomizedListOfTiles.Add(tile);
        }

        for (int i = 0; i < randomizedListOfTiles.Count; i++)
        {
            Tile tile = randomizedListOfTiles[Random.Range(0, randomizedListOfTiles.Count)];

            bool isImpassible = false;
            if (!PlayerSpawns.Contains(tile) && !EnemySpawns.Contains(tile))
            {
                isImpassible = Random.Range(0f, 1f) < impassableChancePerTile;
            }

            tile.SetTerrainObstacle(isImpassible);

            if (isImpassible && !CheckConnectivity(PlayerSpawns[0]) || !CheckConnectivity(EnemySpawns[0]))
            {
                tile.SetTerrainObstacle(false);
                //EnsureConnectivity();
            }

            randomizedListOfTiles.Remove(tile);
        }
    }

    /// <summary>
    /// Generate tiles, not pathfinding nodes.
    /// </summary>
    public void GenerateTiles()
    {
        TilesList.Clear();
        TilesPoints.Clear();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2 gridPoint = new Vector2(x, y);
                if (TilesPoints.Contains(gridPoint) && TilesList[TilesPoints.IndexOf(gridPoint)] != null)
                {
                    DestroyImmediate(TilesList[TilesPoints.IndexOf(gridPoint)].gameObject);
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

                TilesPoints.Add(gridPoint);
                TilesList.Add(tileScript);

                if (x >= 6 && x <= 15 && y >= 7 && y <= 16)
                {
                    tileScript.OutOfBounds = false;
                }
                else
                {
                    tileScript.OutOfBounds = true;
                }
            }
        }


        // for (int x = 6; x <= 15; x++)
        // {
        //     for (int y = 7; y <= 16; y++)
        //     {
        //         TilesList[TilesPoints.IndexOf(new Vector2(x,y))].OutOfBounds = false;
        //         Debug.Log(x + ", " + y + " set in bounds");
        //     }   
        // }
        
    }

    /// <summary>
    /// Get a tile by its position. Accounts for small variances in
    /// the floating point numbers in Vector3 by just getting the closest one.
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public Tile GetClosestTileToPosition(Vector3 pos)
    {
        foreach (Tile t in Tiles.Values)
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
        foreach (var tile in Tiles.Values)
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
        foreach (var tile in Tiles.Values)
        {
            if (tile.IsImpassable)
            {
                impassableTiles.Add(tile);
            }
        }

        for (int i = 0; i < impassableTiles.Count; i++)
        {
            impassableTiles[i].SetTerrainObstacle(false); // Temporarily make passable

            // Check connectivity after clearing this tile
            if (CheckConnectivity(PlayerSpawns[0]))
            {
                break; // Exit if the connectivity is restored
            }
            else
            {
                impassableTiles[i].SetTerrainObstacle(true); // Revert if no path is found
            }
        }
    }
}
