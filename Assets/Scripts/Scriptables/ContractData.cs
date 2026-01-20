using UnityEngine;
using System.Collections.Generic;

// this is being refactored to represent waves.

[CreateAssetMenu(fileName = "ContractData", menuName = "MyScriptables/ContractData")]
public class ContractData : ScriptableObject
{
    public string contractID;
    public string contractTitle;
    public string description;
    public List<GameCharacterData> possibleEnemyTypes;
    public int maxEnemyCount;
    public int minEnemyCount;
    public int waveCount;
}