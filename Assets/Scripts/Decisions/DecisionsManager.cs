using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Events;

public class DecisionsManager : MonoBehaviour
{
    [SerializeField] private Transform _contractsPanelParent;
    [SerializeField] private Transform _recruitPanelParent;
    [SerializeField] private GameObject _winUI;

    [SerializeField] private TMP_Text _goldText;
    [SerializeField] private TMP_Text _payoutText;
    [SerializeField] private TheTabButton _recruitsButton;
    [SerializeField] private TheTabButton _contractsButton;
    [SerializeField] private TheTabButton _troopsButton;
    //[SerializeField] private Button _upgradeContractsButton;
    //[SerializeField] private Button _upgradeRecruitsButton;
    [SerializeField] private GameObject _recruitsScreen;
    [SerializeField] private GameObject _contractsScreen;
    [SerializeField] private GameObject _troopsScreen;
    [SerializeField] private GameObject _levelUpScreen;
    [SerializeField] private GameObject _troopsGrid;
    [SerializeField] private GameObject _troopPrefab;

    [SerializeField] private InventoryPanel _inventoryPanel;

    [SerializeField] private Button _restButton;

    [Header("Shop Stuff")]
    // price associated with upgrading the shop at each level
    [SerializeField] private int[] _shopUpgradeCosts = new int[3];
    [SerializeField] private ShopPanel _shopPanel;
    [SerializeField] private TheTabButton _shopButton;
    [SerializeField] private Button _upgradeShopButton;
    [SerializeField] private TMP_Text _upgradeShopText;

    [Header("Char Detail Panel Stuff")]
    [SerializeField] private CharDetailPanel _charDetail;
    [SerializeField] private Button _disableCharPanelButton;
    [SerializeField] private Button _levelUpButton;
    [SerializeField] private Button _nextCharButton;
    [SerializeField] private Button _lastCharButton;
    [SerializeField] private Button _fireCharacterButton;

    [SerializeField] private GameObject _statLevelUpPopup;

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
        _levelUpButton.onClick.AddListener(TriggerLevelUp);
        _shopButton.TheButton.onClick.AddListener(ShowShopScreen);
        _restButton.onClick.AddListener(Rest);
        _disableCharPanelButton.onClick.AddListener(HideCharacterPanel);
        _nextCharButton.onClick.AddListener(NextCharacterDetails);
        _lastCharButton.onClick.AddListener(LastCharacterDetails);
        _fireCharacterButton.onClick.AddListener(FireCharacter);
        _upgradeShopButton.onClick.AddListener(IncreaseShopLevel);

        _inventoryPanel.OnInventoryItemClick.AddListener(HandleInventoryItemSelected);
        _shopPanel.OnItemPurchased.AddListener(HandleItemPurchased);
        _charDetail.OnItemUnequipped.AddListener(HandleItemUnequipped);

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

    private void OnDestroy()
    {
        _recruitsButton.TheButton.onClick.RemoveListener(ShowRecruitsScreen);
        _contractsButton.TheButton.onClick.RemoveListener(ShowContractsScreen);
        _troopsButton.TheButton.onClick.RemoveListener(ShowTroopsScreen);
        _levelUpButton.onClick.RemoveListener(ShowStatLevelUpPopup);
        _shopButton.TheButton.onClick.RemoveListener(ShowShopScreen);
        _disableCharPanelButton.onClick.RemoveListener(HideCharacterPanel);
        _restButton.onClick.RemoveListener(Rest);
        _nextCharButton.onClick.RemoveListener(NextCharacterDetails);
        _lastCharButton.onClick.RemoveListener(LastCharacterDetails);
        _fireCharacterButton.onClick.RemoveListener(FireCharacter);
        _inventoryPanel.OnInventoryItemClick.RemoveListener(HandleInventoryItemSelected);
        _shopPanel.OnItemPurchased.RemoveListener(HandleItemPurchased);
        _charDetail.OnItemUnequipped.RemoveListener(HandleItemUnequipped);
        _upgradeShopButton.onClick.RemoveListener(IncreaseShopLevel);
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

        RefreshWarband();
        _shopPanel.Refresh();

        UpdateGoldText();
        CheckForGameVictory();


        if (GameManager.Instance.ShopLevel >= _shopUpgradeCosts.Count())
        {
            _upgradeShopButton.gameObject.SetActive(false);
        }

        _upgradeShopText.text = "Upgrade Shop\n(" + _shopUpgradeCosts[GameManager.Instance.ShopLevel] + " Gold)";
    }

    private void RefreshWarband()
    {
        // clear warband stuff
        for (int i = 0; i < _troopsGrid.transform.childCount; i++)
        {
            Destroy(_troopsGrid.transform.GetChild(i).gameObject);
        }

        _charactersToIndex.Clear();
        _indexToCharacters.Clear();

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
    }

    /// <summary>
    /// If the player has enough money, upgrade the shop and refresh it
    /// </summary>
    private void IncreaseShopLevel()
    {
        if (GameManager.Instance.PlayerGold >= _shopUpgradeCosts[GameManager.Instance.ShopLevel])
        {
            GameManager.Instance.AddGold(-_shopUpgradeCosts[GameManager.Instance.ShopLevel]);
            GameManager.Instance.SetShopLevel(GameManager.Instance.ShopLevel + 1);

            _shopPanel.Refresh();
            HandlePlayerUseGold();

            if (GameManager.Instance.ShopLevel >= _shopUpgradeCosts.Count())
            {
                _upgradeShopButton.gameObject.SetActive(false);
            }
            else
            {
                _upgradeShopText.text = "Upgrade Shop\n(" + _shopUpgradeCosts[GameManager.Instance.ShopLevel] + " Gold)";
            }
        }
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

    /// <summary>
    /// Rest, which tells the Game Manager to pass a day and refreshes the UI
    /// </summary>
    private void Rest()
    {
        GameManager.Instance.NextDay(false);
        Refresh();
    }

    private void HandleItemUnequipped(ItemData item)
    {
        GameManager.Instance.PlayerInventory.Add(item);
        _inventoryPanel.AddItem(item);
    }

    public void HandleItemPurchased(ItemData item)
    {
        _inventoryPanel.AddItem(item);
        HandlePlayerUseGold();
    }

    public void HandleInventoryItemSelected(ItemData item)
    {
        // if we're in character detail view then it should equip the item
        if (_charDetail.gameObject.activeInHierarchy)
        {
            // equip character
            ItemData oldItem = _charDetail.EquipItem(item);
            PlaySound(equipSound);

            if (oldItem != null)
            {
                _inventoryPanel.AddItem(oldItem);
            }
        }
        else if (_troopsScreen.activeInHierarchy)
        {
            return;
        }
        // otherwise, if we're looking at the shop, so sell the item 
        else if (_shopPanel.isActiveAndEnabled)
        {
            // give the player money back
            GameManager.Instance.AddGold(item.itemPrice);
            _shopPanel.AddShopItem(item);

            HandlePlayerUseGold();
            CheckForGameVictory();
        }

        // remove item from player inventory data structure
        GameManager.Instance.RemoveItem(item);
    }

    /// <summary>
    /// The player did something involving gold (spend or sell) - play the sound and update UI
    /// </summary>
    public void HandlePlayerUseGold()
    {
        PlaySound(goldSound);
        UpdateGoldText();
    }

    private void PlaySound(AudioClip clip)
    {
        GameObject audioGO = ObjectPool.instance.GetAudioSource();
        audioGO.SetActive(true);
        AudioSource aSource = audioGO.GetComponent<AudioSource>();
        aSource.clip = clip;
        aSource.Play();
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

        RefreshWarband();
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

    private void FireCharacter()
    {
        GameManager.Instance.RemoveFollower(_indexToCharacters[_currentCharToShow]);

        Refresh();

        // If there's no characters to show, close the detail window. Otherwise, cycle to next character.
        if (GameManager.Instance.PlayerFollowers.Count == 0)
        {
            HideCharacterPanel();
        }
        else
        {
            NextCharacterDetails();
        }
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

            _payoutText.text = "Payout: -" + GameManager.Instance.GetPayout(false);
        }
        // just for testing purposes
        else
        {
            _goldText.text = "Gold: " + 200;
        }
    }

    public void TriggerLevelUp()
    {
        if (_indexToCharacters[_currentCharToShow].PendingStatChoices > 0)
        {
            ShowStatLevelUpPopup();
        } 
        else if (_indexToCharacters[_currentCharToShow].PendingPerkChoices > 0)
        {
            ShowPerkLevelUpScreen();
        }
    }

    public void ShowStatLevelUpPopup()
    {
        _statLevelUpPopup.SetActive(true);
    }

    public void ShowCharacterPanel(GameCharacter character)
    {
        // get the index of the character for paging
        _currentCharToShow = _charactersToIndex[character];

        _charDetail.gameObject.SetActive(true);
        _troopsScreen.SetActive(false);
        _levelUpScreen.SetActive(false);
        _inventoryPanel.gameObject.SetActive(true);
        _charDetail.SetCharacter(character);

        if (character.PendingPerkChoices > 0 || character.PendingStatChoices > 0)
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
        _shopPanel.gameObject.SetActive(false);
        _troopsScreen.SetActive(false);
        _inventoryPanel.gameObject.SetActive(false);
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
        _shopPanel.gameObject.SetActive(false);
        _troopsScreen.SetActive(false);
        _inventoryPanel.gameObject.SetActive(false);
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
        _shopPanel.gameObject.SetActive(false);
        _troopsScreen.SetActive(true);
        _inventoryPanel.gameObject.SetActive(true);
        _charDetail.gameObject.SetActive(false);
        _levelUpScreen.gameObject.SetActive(false);

        _troopsButton.SetSelected(true);
        _contractsButton.SetSelected(false);
        _shopButton.SetSelected(false);
        _recruitsButton.SetSelected(false);

        RefreshWarband();
    }

    public void ShowPerkLevelUpScreen()
    {
        _recruitsScreen.SetActive(false);
        _contractsScreen.SetActive(false);
        _shopPanel.gameObject.SetActive(false);
        _troopsScreen.SetActive(false);
        _inventoryPanel.gameObject.SetActive(false);
        _charDetail.gameObject.SetActive(true);
        _levelUpScreen.gameObject.SetActive(true);
    }

    private void ShowShopScreen()
    {
        _recruitsScreen.SetActive(false);
        _contractsScreen.SetActive(false);
        _shopPanel.gameObject.SetActive(true);
        _troopsScreen.SetActive(false);
        _inventoryPanel.gameObject.SetActive(true);
        _charDetail.gameObject.SetActive(false);
        _levelUpScreen.gameObject.SetActive(false);

        _troopsButton.SetSelected(false);
        _contractsButton.SetSelected(false);
        _shopButton.SetSelected(true);
        _recruitsButton.SetSelected(false);
    }
}
