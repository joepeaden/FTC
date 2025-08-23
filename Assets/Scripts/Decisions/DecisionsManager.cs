using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class DecisionsManager : MonoBehaviour
{
    [SerializeField] private Transform _contractsPanelParent;
    [SerializeField] private Transform _recruitPanelParent;
    [SerializeField] private GameObject _winUI;

    [SerializeField] private TMP_Text _goldText;
    [SerializeField] private MyTabButton _recruitsButton;
    [SerializeField] private MyTabButton _contractsButton;
    [SerializeField] private MyTabButton _troopsButton;
    [SerializeField] private MyTabButton _shopButton;
    [SerializeField] private Button _restButton;
    [SerializeField] private Button _upgradeContractsButton;
    [SerializeField] private Button _upgradeRecruitsButton;
    [SerializeField] private Button _upgradeShopButton;
    [SerializeField] private GameObject _recruitsScreen;
    [SerializeField] private GameObject _contractsScreen;
    [SerializeField] private GameObject _troopsScreen;
    [SerializeField] private GameObject _shopScreen;
    [SerializeField] private GameObject _inventoryScreen;
    [SerializeField] private GameObject _levelUpScreen;
    [SerializeField] private GameObject _troopsGrid;
    [SerializeField] private GameObject _shopGrid;
    [SerializeField] private GameObject _inventoryGrid;
    [SerializeField] private GameObject _troopPrefab;
    [SerializeField] private GameObject _itemPrefab;

    [SerializeField] private List<ItemData> possibleShopItems = new();
    private int _maxNumOfShopItems = 10;
    private int _minNumOfShopItems = 6;

    [SerializeField] private CharDetailPanel _charDetail;
    [SerializeField] private Button _disableCharPanelButton;
    [SerializeField] private Button _levelUpButton;
    [SerializeField] private Button _nextCharButton;
    [SerializeField] private Button _lastCharButton;

    private int _currentCharToShow = 0;
    /// <summary>
    /// Dict with character as key and index (child position in _troopsGrid) as value
    /// </summary>
    private Dictionary<GameCharacter, int> _charactersToIndex = new();
    private Dictionary<int, GameCharacter> _indexToCharacters = new();

    [Header("Audio")]
    [SerializeField] private AudioClip goldSound;
    [SerializeField] private AudioClip equipSound;
    

    private void Start()
    {
        _recruitsButton.TheButton.onClick.AddListener(ShowRecruitsScreen);
        _contractsButton.TheButton.onClick.AddListener(ShowContractsScreen);
        _troopsButton.TheButton.onClick.AddListener(ShowTroopsScreen);
        _levelUpButton.onClick.AddListener(ShowLevelUpScreen);
        _shopButton.TheButton.onClick.AddListener(ShowShopScreen);
        _restButton.onClick.AddListener(Rest);
        _disableCharPanelButton.onClick.AddListener(HideCharacterPanel);
        _nextCharButton.onClick.AddListener(NextCharacterDetails);
        _lastCharButton.onClick.AddListener(LastCharacterDetails);

        _charDetail.Setup(this);

        ShowTroopsScreen();
    }

    private void OnEnable()
    {
        for (int i = 0; i < _recruitPanelParent.childCount; i++)
        {
            DecisionPanel panel = _recruitPanelParent.GetChild(i).GetComponent<DecisionPanel>();
            panel.OnRecruit.AddListener(HandleRecruit);
        }

        Refresh();
    }

    private void Refresh()
    {
        // fill out contracts
        for (int i = 0; i < _contractsPanelParent.childCount; i++)
        {
            DecisionPanel panel = _contractsPanelParent.GetChild(i).GetComponent<DecisionPanel>();
            panel.GenerateContractOption();
        }

        // fill out recruits
        for (int i = 0; i < _recruitPanelParent.childCount; i++)
        { 
            DecisionPanel panel = _recruitPanelParent.GetChild(i).GetComponent<DecisionPanel>();
            panel.GenerateRecruitOption();
        }

        // clear warband stuff
        for (int i = 0; i < _troopsGrid.transform.childCount; i++)
        {
            Destroy(_troopsGrid.transform.GetChild(i).gameObject);
        }

        // fill out warband
        if (GameManager.Instance != null)
        {
            int i = 0;
            foreach (GameCharacter character in GameManager.Instance.PlayerFollowers)
            {
                AddCharacterPanel(character);
                _charactersToIndex[character] = i;     // forward lookup
                _indexToCharacters[i] = character;     // reverse lookup

                i++;
            }
        }
        else
        {
            for (int i = 0; i < 6; i++)
            {
                GameObject panelGO = Instantiate(_troopPrefab, _troopsGrid.transform);
            }
        }

        // clear shop items
        for (int i = 0; i < _shopGrid.transform.childCount; i++)
        {
            Destroy(_shopGrid.transform.GetChild(i).gameObject);
        }

        // fill out shop
        int shopItemsCount = Random.Range(_minNumOfShopItems, _maxNumOfShopItems);
        for (int i = 0; i < shopItemsCount; i++)
        {
            ItemData item = possibleShopItems[Random.Range(0, possibleShopItems.Count)];
            GameObject itemGO = Instantiate(_itemPrefab, _shopGrid.transform);
            itemGO.GetComponent<ItemUI>().SetData(item, this, PurchaseItem);
        }

        RefreshInventory();
        UpdateGoldText();
        CheckForGameVictory();
    }

    private void CheckForGameVictory()
    {
        // should this information even be stored in the game manager? I'm not so certain.
        // the player gold isn't used mid-battle anyway. Perhaps should move this here later.
        // I guess it's only there for the sake of persistence, but we can keep that information
        // in a file anyway once we implement perisistence.
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.PlayerGold > GameManager.GOLD_WIN_AMOUNT)
            {
                _winUI.SetActive(true);
                _winUI.GetComponent<GameEndPopup>().SetPlayerWon(true);
            }
            else if (GameManager.Instance.PlayerGold < GameManager.Instance.GameCharData.price && GameManager.Instance.PlayerFollowers.Count == 0) 
            {
                _winUI.SetActive(true);
                _winUI.GetComponent<GameEndPopup>().SetPlayerWon(false);
            }
        }
    }

    private void Rest()
    {
        Refresh();
    }

    public void RefreshInventory()
    {
        // muahahahahaha lazy bug fix  (the tooltip wasn't closing when a gameobjectw as destroyed because
        // of purchase or unequip etc.
        TooltipManager.Instance.HandleCloseTooltip();

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

            PlaySound(equipSound);

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

            PlaySound(goldSound);

            CheckForGameVictory();

            // move the item UI from Shop to Inventory
            itemUI.transform.SetParent(_shopGrid.transform);

            itemUI.RemoveCallbacks();
            itemUI.SetCallback(PurchaseItem);

            UpdateGoldText();
        }
    }

    private void PlaySound(AudioClip clip)
    {
        GameObject audioGO = ObjectPool.instance.GetAudioSource();
        audioGO.SetActive(true);
        AudioSource aSource = audioGO.GetComponent<AudioSource>();
        aSource.clip = clip;
        aSource.Play();
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

        PlaySound(goldSound);

        // move the item UI from Shop to Inventory
        itemUI.transform.SetParent(_inventoryGrid.transform);

        itemUI.RemoveCallbacks();
        itemUI.SetCallback(HandleInventoryItemSelected);

        UpdateGoldText();
    }

    private void AddCharacterPanel(GameCharacter character)
    {
        GameObject panelGO = Instantiate(_troopPrefab, _troopsGrid.transform);
        TroopPanel panel = panelGO.GetComponent<TroopPanel>();
        panel.SetupPanel(character, ShowCharacterPanel);
    }

    private void HideCharacterPanel()
    {
        _troopsScreen.SetActive(true);
        _charDetail.gameObject.SetActive(false);
    }

    private void NextCharacterDetails()
    {
        _currentCharToShow++;

        if (_currentCharToShow > _indexToCharacters.Count - 1)
        {
            _currentCharToShow = 0;
        }

        ShowCharacterPanel(_indexToCharacters[_currentCharToShow]);
    }

    private void LastCharacterDetails()
    {
        _currentCharToShow--;

        if (_currentCharToShow < 0)
        {
            _currentCharToShow = _indexToCharacters.Count - 1;
        }

        ShowCharacterPanel(_indexToCharacters[_currentCharToShow]);
    }

    private void HandleRecruit(GameCharacter newChar)
    {
        UpdateGoldText();
        AddCharacterPanel(newChar);
        PlaySound(goldSound);
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

    public void ShowCharacterPanel(GameCharacter character)
    {
        // get the index of the character for paging
        _currentCharToShow = _charactersToIndex[character];

        _charDetail.gameObject.SetActive(true);
        _troopsScreen.SetActive(false);
        _levelUpScreen.SetActive(false);
        _inventoryScreen.SetActive(true);
        _charDetail.SetCharacter(character);

        if (character.PendingLevelUp)
        {
            _levelUpButton.gameObject.SetActive(true);
        }
        else
        {
            _levelUpButton.gameObject.SetActive(false);
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
        _levelUpScreen.gameObject.SetActive(false);

        _troopsButton.SetSelected(false);
        _contractsButton.SetSelected(false);
        _shopButton.SetSelected(false);
        _recruitsButton.SetSelected(true);
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
        _levelUpScreen.gameObject.SetActive(false);

        _troopsButton.SetSelected(false);
        _contractsButton.SetSelected(true);
        _shopButton.SetSelected(false);
        _recruitsButton.SetSelected(false);
    }

    private void ShowTroopsScreen()
    {
        _recruitsScreen.SetActive(false);
        _contractsScreen.SetActive(false);
        _shopScreen.SetActive(false);
        _troopsScreen.SetActive(true);
        _inventoryScreen.SetActive(true);
        _charDetail.gameObject.SetActive(false);
        _levelUpScreen.gameObject.SetActive(false);

        _troopsButton.SetSelected(true);
        _contractsButton.SetSelected(false);
        _shopButton.SetSelected(false);
        _recruitsButton.SetSelected(false);
    }

    private void ShowLevelUpScreen()
    {
        _recruitsScreen.SetActive(false);
        _contractsScreen.SetActive(false);
        _shopScreen.SetActive(false);
        _troopsScreen.SetActive(false);
        _inventoryScreen.SetActive(false);
        _charDetail.gameObject.SetActive(true);
        _levelUpScreen.gameObject.SetActive(true);
    }

    private void ShowShopScreen()
    {
        _recruitsScreen.SetActive(false);
        _contractsScreen.SetActive(false);
        _shopScreen.SetActive(true);
        _troopsScreen.SetActive(false);
        _inventoryScreen.SetActive(true);
        _charDetail.gameObject.SetActive(false);
        _levelUpScreen.gameObject.SetActive(false);

        _troopsButton.SetSelected(false);
        _contractsButton.SetSelected(false);
        _shopButton.SetSelected(true);
        _recruitsButton.SetSelected(false);
    }

    private void OnDestroy()
    {
        _recruitsButton.TheButton.onClick.RemoveListener(ShowRecruitsScreen);
        _contractsButton.TheButton.onClick.RemoveListener(ShowContractsScreen);
        _troopsButton.TheButton.onClick.RemoveListener(ShowTroopsScreen);
        _levelUpButton.onClick.RemoveListener(ShowLevelUpScreen);
        _shopButton.TheButton.onClick.RemoveListener(ShowShopScreen);
        _disableCharPanelButton.onClick.RemoveListener(HideCharacterPanel);
        _restButton.onClick.RemoveListener(Rest);
        _nextCharButton.onClick.RemoveListener(NextCharacterDetails);
        _lastCharButton.onClick.RemoveListener(LastCharacterDetails);
    }
}
