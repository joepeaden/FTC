using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// PawnSpawner should have responsibility for spawning and tracking waves.
/// </summary>
public class PawnSpawner : MonoBehaviour
{
    public UnityEvent OnReady = new();
    int currentWave;
    private const int DEFAULT_MIN_AMOUNT_TO_SPAWN = 4;
    private const int DEFAULT_MAX_AMOUNT_TO_SPAWN = 7;

    private int spawnPerTurn = 3;

    [SerializeField] GameObject pawnPrefab;
    [SerializeField] Transform enemyParent;
    [SerializeField] Transform friendlyParent;

    private ContractData _contract;

    Stack<GameCharacter> _enemiesForWave = new();

    public void Initialize()
    {
        Dictionary<string, ContractData> contracts = DataLoader.contracts;
        ContractData contract = contracts.Values.ToList()[Random.Range(0, DataLoader.contracts.Count)];
        _contract = contract;

        int numOfEnemies = Random.Range(contract.minEnemyCount, contract.maxEnemyCount);

        _enemiesForWave.Clear();

        int i;
        for (i = 0; i < numOfEnemies; i++)
        {
            GameCharacter guy = new(contract.possibleEnemyTypes[Random.Range(0, contract.possibleEnemyTypes.Count)]);
            _enemiesForWave.Push(guy);
        }

        SpawnPlayerCharacters();

        OnReady.Invoke();
    }

    public void SpawnEnemiesForTurn()
    {
        for (int i = 0; i < spawnPerTurn && _enemiesForWave.Count > 0; i++)
        {
            GameCharacter character = _enemiesForWave.Pop();
            AddNewPawn(character);
        }
    }
    
    public Pawn AddNewPawn(GameCharacter character)
    {
        Transform parent = character.OnPlayerTeam ? friendlyParent : enemyParent;
        Pawn newPawn = Instantiate(pawnPrefab, parent).GetComponent<Pawn>();
        newPawn.SetCharacter(character);

        return newPawn;
    }
    
    public void SpawnPlayerCharacters()
    {
        int numToSpawn = Random.Range(DEFAULT_MIN_AMOUNT_TO_SPAWN, DEFAULT_MAX_AMOUNT_TO_SPAWN);

        for (int i = 0; i < numToSpawn; i++)
        {    
            //         guy.Passives.Add(DataLoader.passives["holy"]);
            //         guy.Abilities.Add(DataLoader.abilities["firstaid"]);
            //         guy.EquipItem(medHelm);
            //         guy.EquipItem(sword);

            GameCharacter guy = new(DataLoader.charTypes["player"]);
        
            AddNewPawn(guy);        
        }
    }
}
