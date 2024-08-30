using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DecisionsManager : MonoBehaviour
{
    [SerializeField] private Transform _contractsPanelParent;
    [SerializeField] private Transform _recruitPanelParent;

    [SerializeField] private TMP_Text _goldText;
    [SerializeField] private Button _recruitsButton;
    [SerializeField] private Button _contractsButton;
    [SerializeField] private Button _troopsButton;
    [SerializeField] private Button _shopButton;
    [SerializeField] private GameObject _recruitsScreen;
    [SerializeField] private GameObject _contractsScreen;
    [SerializeField] private GameObject _troopsScreen;
    [SerializeField] private GameObject _shopScreen;
    [SerializeField] private GameObject _inventoryScreen;
    [SerializeField] private GameObject _troopsGrid;
    [SerializeField] private GameObject _shopGrid;
    [SerializeField] private GameObject _inventoryGrid;
    [SerializeField] private GameObject _troopPrefab;
    [SerializeField] private GameObject _itemPrefab;

    [SerializeField] private List<ItemData> possibleShopItems = new();
    [SerializeField] private int maxNumOfShopItems;
    [SerializeField] private int minNumOfShopItems;

    [SerializeField] private CharDetailPanel _charDetail;
    [SerializeField] private Button _disableCharPanelButton;

    [SerializeField] private EquipmentTooltip equipToolTip;

    private void Awake()
    {
        _recruitsButton.onClick.AddListener(ShowRecruitsScreen);
        _contractsButton.onClick.AddListener(ShowContractsScreen);
        _troopsButton.onClick.AddListener(ShowTroopsScreen);
        _shopButton.onClick.AddListener(ShowShopScreen);

        _charDetail.Setup(this);
    }

    private void OnEnable()
    {
        // fill out contracts
        for (int i = 0; i < _contractsPanelParent.childCount; i++)
        {
            DecisionPanel panel =_contractsPanelParent.GetChild(i).GetComponent<DecisionPanel>();
            panel.GenerateContractOption();
        }

        // fill out recruits
        for (int i = 0; i < _recruitPanelParent.childCount; i++)
        {
            DecisionPanel panel = _recruitPanelParent.GetChild(i).GetComponent<DecisionPanel>();
            panel.GenerateRecruitOption();
            panel.OnRecruit.AddListener(HandleRecruit);
        }

        // fill out warband
        if (GameManager.Instance != null)
        {
            AddCharacterPanel(GameManager.Instance.PlayerCharacter);
            foreach (GameCharacter character in GameManager.Instance.PlayerFollowers)
            {
                AddCharacterPanel(character);
            }
        }
        else
        {
            for (int i = 0; i < 6; i++)
            {
                GameObject panelGO = Instantiate(_troopPrefab, _troopsGrid.transform);
            }
        }

        // fill out shop
        int shopItemsCount = Random.Range(minNumOfShopItems, maxNumOfShopItems);
        for (int i = 0; i < shopItemsCount; i++)
        {
            ItemData item = possibleShopItems[Random.Range(0, possibleShopItems.Count)];
            GameObject itemGO = Instantiate(_itemPrefab, _shopGrid.transform);
            itemGO.GetComponent<ItemUI>().SetData(item, this, PurchaseItem);
        }

        RefreshInventory();

        UpdateGoldText();

        _disableCharPanelButton.onClick.AddListener(HideCharacterPanel);
    }

    public void RefreshInventory()
    {
        for (int i = 0; i < _inventoryGrid.transform.childCount; i++)
        {
            Destroy(_inventoryGrid.transform.GetChild(i).gameObject);
        }

        // fill out inventory
        if (GameManager.Instance != null)
        {
            foreach (ItemData item in GameManager.Instance.PlayerInventory)
            {
                GameObject itemGO = Instantiate(_itemPrefab, _inventoryGrid.transform);
                itemGO.GetComponent<ItemUI>().SetData(item, this, HandleInventoryItemSelected);
            }
        }
    }

    public void HandleInventoryItemSelected(ItemUI itemUI)
    {
        // if we're in character detail view then it should equip the item
        if (_charDetail.gameObject.activeInHierarchy)
        {
            // equip character
            _charDetail.EquipItem(itemUI.Item);

            // remove the item ffrom plyer inventory
            GameManager.Instance.PlayerInventory.Remove(itemUI.Item);

            // refresh UI for inventory
            RefreshInventory();
        }
        // if in troops screen don't do anything
        else if (_troopsScreen.activeInHierarchy)
        {
            return;
        }
        // otherwise, we're looking at the shop, so sell the item 
        else if (_shopScreen.activeInHierarchy)
        {
            // remove item from player inventory data structure
            GameManager.Instance.RemoveItem(itemUI.Item);

            // give the player money back
            GameManager.Instance.AddGold(itemUI.Item.itemPrice);

            // move the item UI from Shop to Inventory
            itemUI.transform.SetParent(_shopGrid.transform);

            itemUI.RemoveCallbacks();
            itemUI.SetCallback(PurchaseItem);

            UpdateGoldText();
        }
    }

    public void PurchaseItem(ItemUI itemUI)
    {
        // add item to player inventory data structure
        bool success = GameManager.Instance.TryAddItem(itemUI.Item);

        // !success can mean we didn't have enough money
        if (!success)
        {
            return;
        }

        // move the item UI from Shop to Inventory
        itemUI.transform.SetParent(_inventoryGrid.transform);

        itemUI.RemoveCallbacks();
        itemUI.SetCallback(HandleInventoryItemSelected);

        UpdateGoldText();
    }

    public void HandleItemHoverStart(ItemData item)
    {
        equipToolTip.SetItem(item);
    }

    public void HandleItemHoverEnd()
    {
        equipToolTip.Hide();
    }

    private void AddCharacterPanel(GameCharacter character)
    {
        GameObject panelGO = Instantiate(_troopPrefab, _troopsGrid.transform);
        TroopPanel panel = panelGO.GetComponent<TroopPanel>();
        panel.SetupPanel(character, ShowCharacterPanel);
    }

    private void ShowCharacterPanel(GameCharacter character)
    {
        _charDetail.gameObject.SetActive(true);
        _troopsScreen.SetActive(false);
        _charDetail.SetCharacter(character);
    }

    private void HideCharacterPanel()
    {
        _troopsScreen.SetActive(true);
        _charDetail.gameObject.SetActive(false);
    }

    private void HandleRecruit(GameCharacter newChar)
    {
        UpdateGoldText();
        AddCharacterPanel(newChar);
    }

    private void UpdateGoldText()
    {
        if (GameManager.Instance != null)
        {
            _goldText.text = "Gold: " + GameManager.Instance.PlayerGold.ToString();
        }
        else
        {
            _goldText.text = "Gold: " + 200;
        }
    }

    private void ShowRecruitsScreen()
    {
        _recruitsScreen.SetActive(true);
        _contractsScreen.SetActive(false);
        _troopsScreen.SetActive(false);
        _shopScreen.SetActive(false);
        _troopsScreen.SetActive(false);
        _inventoryScreen.SetActive(false);
        _charDetail.gameObject.SetActive(false);
    }

    private void ShowContractsScreen()
    {
        _recruitsScreen.SetActive(false);
        _contractsScreen.SetActive(true);
        _troopsScreen.SetActive(false);
        _shopScreen.SetActive(false);
        _troopsScreen.SetActive(false);
        _inventoryScreen.SetActive(false);
        _charDetail.gameObject.SetActive(false);
    }

    private void ShowTroopsScreen()
    {
        _recruitsScreen.SetActive(false);
        _contractsScreen.SetActive(false);
        _shopScreen.SetActive(false);
        _troopsScreen.SetActive(true);
        _inventoryScreen.SetActive(true);
        _charDetail.gameObject.SetActive(false);
    }

    private void ShowShopScreen()
    {
        _recruitsScreen.SetActive(false);
        _contractsScreen.SetActive(false);
        _shopScreen.SetActive(true);
        _troopsScreen.SetActive(false);
        _inventoryScreen.SetActive(true);
        _charDetail.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _recruitsButton.onClick.RemoveListener(ShowRecruitsScreen);
        _contractsButton.onClick.RemoveListener(ShowContractsScreen);
        _troopsButton.onClick.RemoveListener(ShowTroopsScreen);
        _shopButton.onClick.RemoveListener(ShowShopScreen);
        _disableCharPanelButton.onClick.RemoveListener(HideCharacterPanel);
    }
}
