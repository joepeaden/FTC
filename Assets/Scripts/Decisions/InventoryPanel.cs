using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InventoryPanel : MonoBehaviour
{
    [HideInInspector]
    public UnityEvent<ItemData> OnInventoryItemClick = new();

    [SerializeField] private GameObject _inventoryGrid;

    void Start()
    {
        Initialize();
    }


    public void Initialize()
    {
        // muahahahahaha lazy bug fix  (the tooltip wasn't closing when a gameobjectw as destroyed because
        // of purchase or unequip etc.
        //TooltipManager.Instance.HandleCloseTooltip();

        // fill out inventory
        foreach (ItemData item in GameManager.Instance.PlayerInventory)
        {
            AddItem(item);
        }
    }

    public void AddItem(ItemData item)
    {
        GameObject itemGO = ObjectPool.instance.GetItemUI();
        itemGO.transform.SetParent(_inventoryGrid.transform, false);
        itemGO.GetComponent<ItemUI>().SetData(item, HandleInventoryItemSelected);
    }

    public void HandleInventoryItemSelected(ItemUI itemUI)
    {
        OnInventoryItemClick.Invoke(itemUI.Item);

        itemUI.gameObject.SetActive(false);
        itemUI.transform.SetParent(ObjectPool.instance.transform, false);
    }
}
