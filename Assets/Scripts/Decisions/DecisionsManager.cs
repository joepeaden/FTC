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
    [SerializeField] private GameObject _recruitsScreen;
    [SerializeField] private GameObject _contractsScreen;

    private void Awake()
    {
        _recruitsButton.onClick.AddListener(ShowRecruitsScreen);
        _contractsButton.onClick.AddListener(ShowContractsScreen);
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
            panel.OnRecruit.AddListener(UpdateGoldText);
        }

        UpdateGoldText();
    }

    private void UpdateGoldText()
    {
        _goldText.text = "Gold: " + GameManager.Instance.PlayerGold.ToString();
    }

    private void ShowRecruitsScreen()
    {
        _recruitsScreen.SetActive(true);
        _contractsScreen.SetActive(false);
    }

    private void ShowContractsScreen()
    {
        _recruitsScreen.SetActive(false);
        _contractsScreen.SetActive(true);
    }

    private void OnDestroy()
    {
        _recruitsButton.onClick.RemoveListener(ShowRecruitsScreen);
        _contractsButton.onClick.RemoveListener(ShowContractsScreen);
    }
}
