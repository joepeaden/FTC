using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Analytics;

public class Spawner : MonoBehaviour
{
    // singleton
    public static Spawner Instance => _instance;
    private static Spawner _instance;

    // prefabs
    [SerializeField] private TileInhabitant _pawnPrefab;
    [SerializeField] private TileInhabitant _equipmentPrefab;
    [SerializeField] private TileInhabitant _missionBoardPrefab;
    
    // parents
    [SerializeField] private Transform _townParent;
    [SerializeField] private Transform _friendlyParent;
    [SerializeField] private Transform _enemyParent;

    private List<TileInhabitant> spawnedInhabitants = new();

    private void Awake()
    {
        if (_instance != null)
        {
            Debug.Log("Too many Spawners! Disabling this one.");
            gameObject.SetActive(false);
            return;
        }

        _instance = this;
    }

    /// <summary>
    /// Destroy an inhabitant that occupies a tile.
    /// </summary>
    /// <param name="inhabitant"></param>
    /// <param name="removeFromList">Pass false if you don't want to remove the inhabitant from the 
    /// spawnedInhabitants collection. Useful if doing a foreach loop, like DestroyLoadedObjects.</param>
    public void DestroyInhabitant(TileInhabitant inhabitant, bool removeFromList = true)
    {
        if (removeFromList)
        {
            spawnedInhabitants.Remove(inhabitant);
        }

        inhabitant.CurrentTile.ClearInhabitant();
        Destroy(inhabitant.gameObject);
    }

    public void ClearTiles()
    {
        foreach (Tile tile in GridGenerator.Instance.TownShopSpawns)
        {
            tile.SetShopTile(false);
        }

        foreach (Tile tile in GridGenerator.Instance.TownInventorySpawns)
        {
            tile.SetShopTile(false);
        }

        DestroyLoadedObjects();
    }

    public void DestroyLoadedObjects()
    {
        foreach (TileInhabitant inhabitant in spawnedInhabitants)
        {
            DestroyInhabitant(inhabitant, false);
        }

        spawnedInhabitants.Clear();
    }

    private TileInhabitant SpawnInhabitant(TileInhabitant prefab, Transform parent)
    {
        TileInhabitant spawnedObject = Instantiate(prefab, parent);
        spawnedInhabitants.Add(spawnedObject);
        return spawnedObject;
    }

    public void SpawnTown()
    {         
        // spawn followers
        foreach (GameCharacter character in GameManager.Instance.PlayerFollowers)
        {
            Pawn newPawn = SpawnInhabitant(_pawnPrefab, _townParent).GetComponent<Pawn>();
            // _playerPawns.Add(newPawn);
            newPawn.SetCharacter(character);
            PickPawnSpawnTileTown(newPawn, GridGenerator.Instance.TownFollowerSpawns);
        }

        // spawn recruits
        for (int i = 0; i < 5; i++)
        {
            Pawn newPawn = SpawnInhabitant(_pawnPrefab, _townParent).GetComponent<Pawn>();
            // _playerPawns.Add(newPawn);
            newPawn.SetCharacter(new GameCharacter(DataLoader.charTypes["player"]));
            newPawn.SetAltShirt(true);
            PickPawnSpawnTileTown(newPawn, GridGenerator.Instance.TownRecruitSpawns);
        }

        TileInhabitant missionBoard = SpawnInhabitant(_missionBoardPrefab, _townParent);
        GridGenerator.Instance.TownMissionBoardSpawn.SetInhabitant(missionBoard.GetComponent<TileInhabitant>());
        missionBoard.transform.position = GridGenerator.Instance.TownMissionBoardSpawn.transform.position;

        foreach (Tile t in GridGenerator.Instance.TownShopSpawns)
        {
            t.SetShopTile(true);
        }

        foreach (Tile t in GridGenerator.Instance.TownInventorySpawns)
        {
            t.SetShopTile(true);
        }
    }

    public void SpawnTownItem(ItemData item, Tile tile, bool playerOwns)
    {
        TileInhabitant sampleItem = SpawnInhabitant(_equipmentPrefab, _townParent);
        ItemUIImmersive immersiveItem = sampleItem.GetComponent<ItemUIImmersive>();
        immersiveItem.SetData(item, playerOwns);
        tile.SetInhabitant(immersiveItem);

        sampleItem.transform.position = tile.transform.position;
    }

    private void PickPawnSpawnTileTown(Pawn p, List<Tile> spawnPoints)
    {   
        foreach(Tile t in spawnPoints)
        {
            if (t.GetInhabitant() == null)
            {
                p.PlaceAtTile(t);
                return;
            }
        } 

        Debug.Log("Not enough spawn tiles.");
    }


    private void PickStartTileBattle(Pawn p)
    {
        Tile spawnTile;
        if (p.OnPlayerTeam)
        {
            do
            {
                spawnTile = GridGenerator.Instance.PlayerSpawns[Random.Range(0, GridGenerator.Instance.PlayerSpawns.Count)];
            } while (spawnTile.GetInhabitant() != null);
        }
        else
        {
            do
            {
                spawnTile = GridGenerator.Instance.EnemySpawns[Random.Range(0, GridGenerator.Instance.EnemySpawns.Count)];
            } while (spawnTile.GetInhabitant() != null);
        }

        p.PlaceAtTile(spawnTile);
    }


    public void Spawn()
    {


        // if not started from Battle scene, spawn player's company and enemies in contract
        // if (GameManager.Instance != null)
        // {
        //     //dudesToSpawn = GameManager.Instance.GetNumOfEnemiesToSpawn();

        //     foreach (GameCharacter character in GameManager.Instance.PlayerFollowers)
        //     {
        //         Pawn newPawn = SpawnInhabitant(_pawnPrefab, _friendlyParent).GetComponent<Pawn>();
        //         // _playerPawns.Add(newPawn);
        //         newPawn.SetCharacter(character);

        //         MiniStatBar miniStats = SpawnInhabitant(_miniStatBarPrefab, _healthBarParent);
        //         miniStats.SetData(newPawn);
        //     }

        //     foreach(GameCharacter character in GameManager.Instance.GetEnemiesForContract())
        //     {
        //         Pawn newPawn = SpawnInhabitant(_pawnPrefab, _enemyParent).GetComponent<Pawn>();
        //         newPawn.SetCharacter(character);

        //         // _aiPlayer.RegisterPawn(newPawn);

        //         MiniStatBar miniStats = SpawnInhabitant(_miniStatBarPrefab, _healthBarParent);
        //         miniStats.SetData(newPawn);
        //     }
        // }
        // otherwise, spawn a random assortment of friendly and enemy dudes
        // else
        // {
        //     StartCoroutine(TestModeOnDataLoadedStart());
        // }
    }

}
