using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Collections;
using System.Linq;

public class ShopPanel : MonoBehaviour
{
    [HideInInspector]
    public UnityEvent<ItemData> OnItemPurchased = new();

    [SerializeField] private GameObject _shopGrid;
    private List<ItemData> possibleItems;
    private int _maxNumOfItems = 10;
    private int _minNumOfItems = 6;

    /// <summary>
    /// Get store items from data loader based on the current store level
    /// </summary>
    private void GetItemsForStoreLevel()
    {
        // get all items that should be in the store (-1 should never be sold)
        // and that are available at the current store level
        possibleItems = DataLoader.weapons.Values.Where(x => x.storeLevel >= 0 && x.storeLevel <= GameManager.Instance.ShopLevel).ToList();
        possibleItems.AddRange(DataLoader.armor.Values.Where(x => x.storeLevel >= 0 && x.storeLevel <= GameManager.Instance.ShopLevel).ToList());
    }

    /// <summary>
    /// Return everything to object pool and re-randomly pick new items and display them in the grid
    /// </summary>
    public void Refresh()
    {
        GetItemsForStoreLevel();

        for (int i = 0; i < _shopGrid.transform.childCount; i++)
        {
            ObjectPool.instance.Return(_shopGrid.transform.GetChild(i).gameObject);
        }

        // fill out shop
        int shopItemsCount = Random.Range(_minNumOfItems, _maxNumOfItems);
        for (int i = 0; i < shopItemsCount; i++)
        {
            ItemData item = possibleItems[Random.Range(0, possibleItems.Count)];
            AddShopItem(item);
        }
    }

    public void AddShopItem(ItemData item)
    {
        GameObject itemGO = ObjectPool.instance.GetItemUI();
        itemGO.transform.parent = _shopGrid.transform;
        itemGO.GetComponent<ItemUI>().SetData(item, PurchaseItem);
    }

    public void PurchaseItem(ItemUI itemUI)
    {
        // add item to player inventory data structure
        bool success = GameManager.Instance.TryPurchaseItem(itemUI.Item);

        // !success can mean we didn't have enough money
        if (!success)
        {
            return;
        }

        OnItemPurchased.Invoke(itemUI.Item);

        ObjectPool.instance.Return(itemUI.gameObject);
    }
}
