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

    private void Awake()
    {
        _recruitsButton.onClick.AddListener(ShowRecruitsScreen);
        _contractsButton.onClick.AddListener(ShowContractsScreen);
        _troopsButton.onClick.AddListener(ShowTroopsScreen);
        _shopButton.onClick.AddListener(ShowShopScreen);
    }

    private void OnEnable()
    {
        for (int i = 0; i < _contractsPanelParent.childCount; i++)
        {
            DecisionPanel panel =_contractsPanelParent.GetChild(i).GetComponent<DecisionPanel>();
            panel.GenerateContractOption();
        }

        for (int i = 0; i < _recruitPanelParent.childCount; i++)
        {
            DecisionPanel panel = _recruitPanelParent.GetChild(i).GetComponent<DecisionPanel>();
            panel.GenerateRecruitOption();
            panel.OnRecruit.AddListener(HandleRecruit);
        }

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

        for (int i = 0; i < 6; i++)
        {
            GameObject panelGO = Instantiate(_itemPrefab, _shopGrid.transform);
        }

        for (int i = 0; i < 6; i++)
        {
            GameObject panelGO = Instantiate(_itemPrefab, _inventoryGrid.transform);
        }

        UpdateGoldText();
    }

    private void AddCharacterPanel(GameCharacter character)
    {
        GameObject panelGO = Instantiate(_troopPrefab, _troopsGrid.transform);
        TroopPanel panel = panelGO.GetComponent<TroopPanel>();
        panel.SetupPanel(character);
    }

    private void HandleRecruit(GameCharacter newChar)
    {
        UpdateGoldText();
        AddCharacterPanel(newChar);
    }

    private void UpdateGoldText()
    {
        _goldText.text = "Gold: " + GameManager.Instance.PlayerGold.ToString();
    }

    private void ShowRecruitsScreen()
    {
        _recruitsScreen.SetActive(true);
        _contractsScreen.SetActive(false);
        _troopsScreen.SetActive(false);
        _shopScreen.SetActive(false);
        _troopsScreen.SetActive(false);
        _inventoryScreen.SetActive(false);
    }

    private void ShowContractsScreen()
    {
        _recruitsScreen.SetActive(false);
        _contractsScreen.SetActive(true);
        _troopsScreen.SetActive(false);
        _shopScreen.SetActive(false);
        _troopsScreen.SetActive(false);
        _inventoryScreen.SetActive(false);
    }

    private void ShowTroopsScreen()
    {
        _recruitsScreen.SetActive(false);
        _contractsScreen.SetActive(false);
        _shopScreen.SetActive(false);
        _troopsScreen.SetActive(true);
        _inventoryScreen.SetActive(true);
    }

    private void ShowShopScreen()
    {
        _recruitsScreen.SetActive(false);
        _contractsScreen.SetActive(false);
        _shopScreen.SetActive(true);
        _troopsScreen.SetActive(false);
        _inventoryScreen.SetActive(true);
    }

    private void OnDestroy()
    {
        _recruitsButton.onClick.RemoveListener(ShowRecruitsScreen);
        _contractsButton.onClick.RemoveListener(ShowContractsScreen);
        _troopsButton.onClick.RemoveListener(ShowTroopsScreen);
        _shopButton.onClick.RemoveListener(ShowShopScreen);
    }
}
