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
        _confirmButton.onClick.AddListener(ApplyChanges);
    }

    private void OnDestroy()
    {
        _confirmButton.onClick.RemoveListener(ApplyChanges);
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void Refresh()
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
    }

    public void HandleLvlUpCardSelected(LevelUpCard card)
    {
        if (!_detailPanel.CurrentCharacter.Abilities.Contains(card.ability))
        {
            if (card.ability != null)
            {
                _detailPanel.CurrentCharacter.Abilities.Add(card.ability);
            }
            else if (card.passive != null)
            {
                _detailPanel.CurrentCharacter.Passives.Add(card.passive);
            }
        }

        _detailPanel.CurrentCharacter.PendingPerkChoices--;

        ApplyChanges();
    }

    private void ApplyChanges()
    {
        // refresh the detail panel w/ new stats
        _decisionsManager.ShowCharacterPanel(_detailPanel.CurrentCharacter);

        // if there's still perk points, let the player pick more.
        if (_detailPanel.CurrentCharacter.PendingPerkChoices > 0)
        {
            _decisionsManager.ShowPerkLevelUpScreen();
        }
    }
}