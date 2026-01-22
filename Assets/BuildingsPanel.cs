using UnityEngine;

public class BuildingsPanel : MonoBehaviour
{
    private int numberOfBuildingSlots = 3;

    [SerializeField] private GameObject buildingPrefab;
    [SerializeField] private GameObject buildingSlotPrefab;

    [SerializeField] Transform buildingsParent;

    private void OnEnable()
    {
        for (int i = 0; i < numberOfBuildingSlots; i++)
        {
            Instantiate(buildingSlotPrefab, buildingsParent);
        } 
    }
}
