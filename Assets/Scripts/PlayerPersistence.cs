using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This class should represent the player's unlocks, etc. Stuff that the player "has".
/// </summary>
public class PlayerPersistence : MonoBehaviour
{
    public List<BuildingData> UnlockedBuildings => _unlockedBuildings;
    private List<BuildingData> _unlockedBuildings = new();
}
