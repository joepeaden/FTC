using UnityEngine;

/// <summary>
/// PawnSpawner should have responsibility for spawning and tracking waves.
/// </summary>
public class PawnSpawner : MonoBehaviour
{
    int currentWave;
    private const int DEFAULT_MIN_AMOUNT_TO_SPAWN = 4;
    private const int DEFAULT_MAX_AMOUNT_TO_SPAWN = 7;

    [SerializeField] GameObject pawnPrefab;
    [SerializeField] Transform enemyParent;
    [SerializeField] Transform friendlyParent;

    public Pawn AddNewPawn(GameCharacter character)
    {
        Transform parent = character.OnPlayerTeam ? friendlyParent : enemyParent;
        Pawn newPawn = Instantiate(pawnPrefab, parent).GetComponent<Pawn>();
        newPawn.SetCharacter(character);

        return newPawn;
    }
    
    public void SpawnTestGuys()
    {
        SpawnTestGuys(true);
        SpawnTestGuys(false);
    }

    private void SpawnTestGuys(bool friendly)
    {
        int numToSpawn = Random.Range(DEFAULT_MIN_AMOUNT_TO_SPAWN, DEFAULT_MAX_AMOUNT_TO_SPAWN);

        for (int i = 0; i < numToSpawn; i++)
        {    
            //         guy.Passives.Add(DataLoader.passives["holy"]);
            //         guy.Abilities.Add(DataLoader.abilities["firstaid"]);
            //         guy.EquipItem(medHelm);
            //         guy.EquipItem(sword);

            GameCharacter guy = new(friendly ? DataLoader.charTypes["player"] : DataLoader.charTypes["warrior"]);
        
            AddNewPawn(guy);
            
        }
    }


}
