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
    [SerializeField] private GameObject _recruitsScreen;
    [SerializeField] private GameObject _contractsScreen;
    [SerializeField] private GameObject _troopsScreen;
    [SerializeField] private GameObject _troopPanelPrefab;

    private void Awake()
    {
        _recruitsButton.onClick.AddListener(ShowRecruitsScreen);
        _contractsButton.onClick.AddListener(ShowContractsScreen);
        _troopsButton.onClick.AddListener(ShowTroopsScreen);
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

        AddCharacterPanel(GameManager.Instance.PlayerCharacter);
        foreach (CharInfo character in GameManager.Instance.PlayerFollowers)
        {
            AddCharacterPanel(character);
        }

        UpdateGoldText();
    }

    private void AddCharacterPanel(CharInfo character)
    {
        GameObject panelGO = Instantiate(_troopPanelPrefab, _troopsScreen.transform);
        TroopPanel panel = panelGO.GetComponent<TroopPanel>();
        panel.SetupPanel(character);
    }

    private void HandleRecruit(CharInfo newChar)
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
    }

    private void ShowContractsScreen()
    {
        _recruitsScreen.SetActive(false);
        _contractsScreen.SetActive(true);
        _troopsScreen.SetActive(false);
    }

    private void ShowTroopsScreen()
    {
        _recruitsScreen.SetActive(false);
        _contractsScreen.SetActive(false);
        _troopsScreen.SetActive(true);
    }

    private void OnDestroy()
    {
        _recruitsButton.onClick.RemoveListener(ShowRecruitsScreen);
        _contractsButton.onClick.RemoveListener(ShowContractsScreen);
        _troopsButton.onClick.RemoveListener(ShowTroopsScreen);
    }
}
