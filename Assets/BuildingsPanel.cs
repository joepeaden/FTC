using UnityEngine;

public class BuildingsPanel : MonoBehaviour
{
    [SerializeField] private PlayerPersistence _playerPersist;
    [SerializeField] private Castle _castle;
    [SerializeField] private GameObject buildingPrefab;
    [SerializeField] private GameObject buildingSlotPrefab;
    [SerializeField] Transform buildingsParent;


    private void OnEnable()
    {
        // optimize later

        for (int i = 0; i < buildingsParent.childCount; i++)
        {
            Destroy(buildingsParent.GetChild(i).gameObject);            
        }

        foreach (BuildingData building in _castle.Buildings)
        {
            Instantiate(buildingPrefab, buildingsParent);
        }

        for (int i = 0; i < _castle.GetBuildingSlots(); i++)
        {
            Instantiate(buildingSlotPrefab, buildingsParent);
        } 
    }

    // private void ShowAvailableBuildings()
    // {
    //     _playerPersist.UnlockedBuildings       
    // }
}
