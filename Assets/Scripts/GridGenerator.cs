using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] GameObject[] tilePrefabs;
    [SerializeField] int gridHeight;
    [SerializeField] int gridWidth;
    [SerializeField] float tileSize;

    public Dictionary<Point, Tile> Tiles => _tiles;
    private Dictionary<Point, Tile> _tiles = new();

    // Start is called before the first frame update
    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        Character c = FindAnyObjectByType<Character>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GameObject tilePrefab = tilePrefabs[Random.Range(0, tilePrefabs.Length)];
                GameObject tileGO = Instantiate(tilePrefab, transform);

                float posX = (x * tileSize + y * tileSize) / 2f;
                float posY = (x * tileSize - y * tileSize) / 4f;

                tileGO.transform.position = new Vector3(posX, posY);
                tileGO.name = "Tile (" + posX + ", " + posY + ")";

                // temporary HOPEFULLY!
                if (x == 0 && y == 0)
                {
                    tileGO.GetComponent<Tile>().CharacterEnterTile(c);
                }

                _tiles[new Point(x, y)] = tileGO.GetComponent<Tile>();
            }
        }

        foreach (KeyValuePair<Point, Tile> kvp in _tiles)
        {
            Tile tile = kvp.Value;
            Point coord = kvp.Key;

            tile.Initialize(_tiles, coord);
        }

        _tiles[new Point(0, 0)].CheckIfInRange(c.MoveRange, true);
    }
}
