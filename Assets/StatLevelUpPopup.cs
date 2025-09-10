using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatLevelUpPopup : MonoBehaviour
{
    [SerializeField] private DecisionsManager _decisions;
    [SerializeField] private CharDetailPanel _detailPanel;

    [SerializeField] private PipStatBar _HPBar;
    [SerializeField] private TMP_Text _MoveText;
    [SerializeField] private PipStatBar _MotBar;
    [SerializeField] private TMP_Text _InitText;
    [SerializeField] private TMP_Text _AccText;
    [SerializeField] private TMP_Text _CritChanceText;

    [SerializeField] private Button _improveHPButton;
    [SerializeField] private Button _improveMoveButton;
    [SerializeField] private Button _improveMotButton;
    [SerializeField] private Button _improveInitButton;
    [SerializeField] private Button _improveAccButton;
    [SerializeField] private Button _improveCritChanceButton;
    [SerializeField] private Button _acceptSkillImprovementButton;
    [SerializeField] private Button _resetSkillsImprovementButton;

    private GameCharacter _currentCharacter;

    private int _addHP;
    private int _addMove;
    private int _addInit;
    private int _addAcc;
    private int _addMot;
    private int _addCritChance;

    public void Awake()
    {
        _improveHPButton.onClick.AddListener(BoostHP);
        _improveAccButton.onClick.AddListener(BoostAcc);
        _improveMoveButton.onClick.AddListener(BoostMove);
        _improveMotButton.onClick.AddListener(BoostMot);
        _improveCritChanceButton.onClick.AddListener(BoostCritChance);
        _improveInitButton.onClick.AddListener(BoostInit);
        _acceptSkillImprovementButton.onClick.AddListener(AcceptSkillImprovement);
        _resetSkillsImprovementButton.onClick.AddListener(Reset);
    }

    public void OnDestroy()
    {
        _improveHPButton.onClick.RemoveListener(BoostHP);
        _improveAccButton.onClick.RemoveListener(BoostAcc);
        _improveMoveButton.onClick.RemoveListener(BoostMove);
        _improveMotButton.onClick.RemoveListener(BoostMot);
        _improveCritChanceButton.onClick.RemoveListener(BoostCritChance);
        _improveInitButton.onClick.RemoveListener(BoostInit);
        _acceptSkillImprovementButton.onClick.RemoveListener(AcceptSkillImprovement);
        _resetSkillsImprovementButton.onClick.RemoveListener(Reset);
    }

    public void OnEnable()
    {
        _currentCharacter = _detailPanel.CurrentCharacter;
        Reset();
    }

    private void Refresh()
    {
        _HPBar.SetBar(_currentCharacter.HitPoints);
        _MotBar.SetBar(_currentCharacter.GetBattleMotivationCap());
        _CritChanceText.text = _currentCharacter.CritChance.ToString();
        _MoveText.text = _currentCharacter.GetMoveRange().ToString();
        _InitText.text = _currentCharacter.BaseInitiative.ToString();
        _AccText.text = _currentCharacter.AccRating.ToString();

        SetImproveButtonsActive(true);
    }

    private void Reset()
    {
        _addHP = 0;
        _addMove = 0;
        _addInit = 0;
        _addAcc = 0;
        _addMot = 0;
        _addCritChance = 0;

        Refresh();
    }

    private void AcceptSkillImprovement()
    {
        _detailPanel.CurrentCharacter.ChangeAcc(_addAcc);
        _detailPanel.CurrentCharacter.ChangeMove(_addMove);
        _detailPanel.CurrentCharacter.ChangeCritChance(_addCritChance);
        _detailPanel.CurrentCharacter.ChangeHP(_addHP);
        _detailPanel.CurrentCharacter.ChangeInit(_addInit);
        _detailPanel.CurrentCharacter.ChangeMot(_addMot);

        _decisions.ShowCharacterPanel(_detailPanel.CurrentCharacter);
        _decisions.ShowPerkLevelUpScreen();

        gameObject.SetActive(false);
    }

    private void SetImproveButtonsActive(bool enabled)
    {
        _improveHPButton.gameObject.SetActive(enabled);
        _improveAccButton.gameObject.SetActive(enabled);
        _improveMoveButton.gameObject.SetActive(enabled);
        _improveMotButton.gameObject.SetActive(enabled);
        _improveCritChanceButton.gameObject.SetActive(enabled);
        _improveInitButton.gameObject.SetActive(enabled);
    }

    private void BoostAcc()
    {
        _addAcc++;
        _AccText.text = (_currentCharacter.AccRating + _addAcc).ToString();
        SetImproveButtonsActive(false);
    }

    private void BoostHP()
    {
        _addHP++;
        _HPBar.SetBar(_currentCharacter.HitPoints + _addHP);
        SetImproveButtonsActive(false);
    }

    private void BoostMove()
    {
        _addMove++;
        _MoveText.text = (_currentCharacter.GetMoveRange() + _addMove).ToString();
        SetImproveButtonsActive(false);
    }

    private void BoostCritChance()
    {
        _addCritChance--;
        _CritChanceText.text = (_currentCharacter.CritChance + _addCritChance).ToString();
        SetImproveButtonsActive(false);
    }

    private void BoostMot()
    {
        _addMot++;
        _MotBar.SetBar(_currentCharacter.GetBattleMotivationCap() + _addMot);
        SetImproveButtonsActive(false);
    }

    private void BoostInit()
    {
        _addInit++;
        _InitText.text = (_currentCharacter.BaseInitiative + _addInit).ToString();
        SetImproveButtonsActive(false);
    }
}
