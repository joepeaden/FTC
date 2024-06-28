using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public static GridGenerator Instance => _instance;
    private static GridGenerator _instance;

    [SerializeField] GameObject[] tilePrefabs;
    [SerializeField] int gridHeight;
    [SerializeField] int gridWidth;
    [SerializeField] float tileSize;

    private Dictionary<Point, Tile> _tiles = new();
    public List<Tile> PlayerSpawns => _playerSpawns;
    public List<Tile> EnemySpawns => _enemySpawns;
    private List<Tile> _playerSpawns = new();
    private List<Tile> _enemySpawns = new();

    // Start is called before the first frame update
    void Awake()
    {
        _instance = this;

        GenerateGrid();
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

            tile.Initialize(_tiles, coord);
        }
    }
}
