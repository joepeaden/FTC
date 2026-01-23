using UnityEngine;

// this is being refactored to represent waves.

[CreateAssetMenu(fileName = "BuildingData", menuName = "MyScriptables/BuildingData")]
public class BuildingData : ScriptableObject
{
    public string ID;
    public string contractTitle;
    public string description;
    public int cost;
    public Sprite sprite;
    public GameObject prefab;
}