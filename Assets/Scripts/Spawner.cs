using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Analytics;

public class Spawner : MonoBehaviour
{
    // singleton
    public static Spawner Instance => _instance;
    private static Spawner _instance;

    // prefabs
    [SerializeField] private GameObject _pawnPrefab;
    [SerializeField] private GameObject _equipmentPrefab;
    
    // parents
    [SerializeField] private Transform _townParent;
    [SerializeField] private Transform _friendlyParent;
    [SerializeField] private Transform _enemyParent;

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

    public void SpawnTown()
    {         
        // spawn followers
        foreach (GameCharacter character in GameManager.Instance.PlayerFollowers)
        {
            Pawn newPawn = Instantiate(_pawnPrefab, _townParent).GetComponent<Pawn>();
            // _playerPawns.Add(newPawn);
            newPawn.SetCharacter(character);
            PickPawnSpawnTileTown(newPawn, GridGenerator.Instance.TownFollowerSpawns);
        }

        // spawn recruits
        for (int i = 0; i < 5; i++)
        {
            Pawn newPawn = Instantiate(_pawnPrefab, _townParent).GetComponent<Pawn>();
            // _playerPawns.Add(newPawn);
            newPawn.SetCharacter(new GameCharacter(DataLoader.charTypes["player"]));
            newPawn.SetAltShirt(true);
            PickPawnSpawnTileTown(newPawn, GridGenerator.Instance.TownRecruitSpawns);
        }
    }

    public void SpawnTownItem(ItemData item, Tile tile, bool playerOwns)
    {
        GameObject sampleItem = Instantiate(_equipmentPrefab, _townParent);
        ItemUIImmersive immersiveItem = sampleItem.GetComponent<ItemUIImmersive>();
        immersiveItem.SetData(item, playerOwns);
        tile.SetItem(immersiveItem);

        sampleItem.transform.position = tile.transform.position;
    }

    private void PickPawnSpawnTileTown(Pawn p, List<Tile> spawnPoints)
    {   
        foreach(Tile t in spawnPoints)
        {
            if (t.GetPawn() == null)
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
            } while (spawnTile.GetPawn() != null);
        }
        else
        {
            do
            {
                spawnTile = GridGenerator.Instance.EnemySpawns[Random.Range(0, GridGenerator.Instance.EnemySpawns.Count)];
            } while (spawnTile.GetPawn() != null);
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
        //         Pawn newPawn = Instantiate(_pawnPrefab, _friendlyParent).GetComponent<Pawn>();
        //         // _playerPawns.Add(newPawn);
        //         newPawn.SetCharacter(character);

        //         MiniStatBar miniStats = Instantiate(_miniStatBarPrefab, _healthBarParent);
        //         miniStats.SetData(newPawn);
        //     }

        //     foreach(GameCharacter character in GameManager.Instance.GetEnemiesForContract())
        //     {
        //         Pawn newPawn = Instantiate(_pawnPrefab, _enemyParent).GetComponent<Pawn>();
        //         newPawn.SetCharacter(character);

        //         // _aiPlayer.RegisterPawn(newPawn);

        //         MiniStatBar miniStats = Instantiate(_miniStatBarPrefab, _healthBarParent);
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
