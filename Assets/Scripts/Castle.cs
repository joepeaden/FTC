using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class Castle : MonoBehaviour
{
    private int _buildingSlots = 3;

    public List<BuildingData> Buildings => _buildings;
    private List<BuildingData> _buildings = new();

    // the integer represents remaining health
    public UnityEvent<int> OnGetHit = new();

    [SerializeField] private int _remainingHitPoints;

    public void GetHit(int dmg)
    {
        _remainingHitPoints -= dmg;
        OnGetHit.Invoke(_remainingHitPoints);
    } 

    public void AddBuilding(BuildingData buildingToAdd)
    {
        _buildings.Add(buildingToAdd);
    }

    public int GetBuildingSlots()
    {
        return _buildingSlots - _buildings.Count;
    }
}
