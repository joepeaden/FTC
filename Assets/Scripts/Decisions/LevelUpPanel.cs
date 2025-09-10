using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

/// <summary>
/// Handles UI for level up screen, and can apply changes and refresh
/// CharDetailPanel UI.
/// </summary>
public class LevelUpPanel : MonoBehaviour
{
    [SerializeField] private Button _increaseHP;
    [SerializeField] private Button _increaseAccRating;
    [SerializeField] private PipStatBar _hpLevelUpStatBar;
    [SerializeField] private TMP_Text _accLevelUpText;
    [SerializeField] private TMP_Text _statPoints;
    [SerializeField] private Button _confirmButton;

    // needed to update current character and also tell detail panel to update
    [SerializeField] private CharDetailPanel _detailPanel;
    [SerializeField] private DecisionsManager _decisionsManager;

    [SerializeField] private List<LevelUpCard> _cards = new();

    private void Start()
    {
        _increaseAccRating.onClick.AddListener(ImproveAccRating);
        _increaseHP.onClick.AddListener(ImproveHP);
        _confirmButton.onClick.AddListener(ApplyChanges);
    }

    private void OnDestroy()
    {
        _increaseAccRating.onClick.RemoveListener(ImproveAccRating);
        _increaseHP.onClick.RemoveListener(ImproveHP);
        _confirmButton.onClick.RemoveListener(ApplyChanges);
    }

    private void OnEnable()
    {
        foreach (LevelUpCard card in _cards)
        {
            if (_detailPanel.CurrentCharacter.Level == 1)
            {
                List<PassiveData> lvl1Passives = DataLoader.passives.Values.Where(x => x.level == 1).ToList();
                PassiveData cardPassive = lvl1Passives[Random.Range(0, lvl1Passives.Count)];
                card.SetData(cardPassive, this);
            }
            else if (_detailPanel.CurrentCharacter.Level == 2)
            {
                Ability cardAbility = DataLoader.abilities.Values.ToList()[Random.Range(0, DataLoader.abilities.Count)];
                card.SetData(cardAbility, this);
            }
            else if (_detailPanel.CurrentCharacter.Level == 3)
            {
                List<PassiveData> lvl3Passives = DataLoader.passives.Values.Where(x => x.level == 3).ToList();
                PassiveData cardPassive = lvl3Passives[Random.Range(0, lvl3Passives.Count)];
                card.SetData(cardPassive, this);
            }
        }

        //CheckStatCapOrPointsSpent();

        //_hpLevelUpStatBar.SetBar(_detailPanel.CurrentCharacter.HitPoints);
        //_accLevelUpText.text = _detailPanel.CurrentCharacter.AccRating + "+";
    }

    private void ImproveAccRating()
    {
        _detailPanel.CurrentCharacter.ChangeAcc(-1);
        _accLevelUpText.text = _detailPanel.CurrentCharacter.AccRating + "+";
        _detailPanel.CurrentCharacter.SpendLevelUp();

        CheckStatCapOrPointsSpent();
    }

    private void ImproveHP()
    {
        _detailPanel.CurrentCharacter.ChangeHP(1);
        _hpLevelUpStatBar.SetBar(_detailPanel.CurrentCharacter.HitPoints);
        _detailPanel.CurrentCharacter.SpendLevelUp();

        CheckStatCapOrPointsSpent();
    }

    private void CheckStatCapOrPointsSpent()
    {
        bool hpAtMaxVal = _detailPanel.CurrentCharacter.HitPoints >= 8;

        _increaseAccRating.gameObject.SetActive(_detailPanel.CurrentCharacter.PendingLevelUp);
        _increaseHP.gameObject.SetActive(!hpAtMaxVal && _detailPanel.CurrentCharacter.PendingLevelUp);

        _statPoints.text = "Stat Points: " + _detailPanel.CurrentCharacter.PendingLevelUp;

    }

    public void HandleLvlUpCardSelected(LevelUpCard card)
    {
        if (card.ability != null)
        {
            _detailPanel.CurrentCharacter.Abilities.Add(card.ability);
        }
        else if (card.passive != null)
        {
            _detailPanel.CurrentCharacter.Passives.Add(card.passive);
        }

        _detailPanel.CurrentCharacter.SpendLevelUp();

        ApplyChanges();
    }

    private void ApplyChanges()
    {
        // refresh the detail panel w/ new stats
        //_detailPanel.SetCharacter(_detailPanel.CurrentCharacter);
        _decisionsManager.ShowCharacterPanel(_detailPanel.CurrentCharacter);
    }
}