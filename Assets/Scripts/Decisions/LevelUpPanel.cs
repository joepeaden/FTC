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
        //_detailPanel

        foreach (LevelUpCard card in _cards)
        {
            Ability cardAbility = DataLoader.abilities.Values.ToList()[Random.Range(0, DataLoader.abilities.Count)];
            card.SetData(cardAbility, this);
        }

        //CheckStatCapOrPointsSpent();

        //_hpLevelUpStatBar.SetBar(_detailPanel.CurrentCharacter.HitPoints);
        //_accLevelUpText.text = _detailPanel.CurrentCharacter.AccRating + "+";
    }

    private void ImproveAccRating()
    {
        _detailPanel.CurrentCharacter.ChangeAccRating(-1);
        _accLevelUpText.text = _detailPanel.CurrentCharacter.AccRating + "+";
        _detailPanel.CurrentCharacter.SpendStatPoint();

        CheckStatCapOrPointsSpent();
    }

    private void ImproveHP()
    {
        _detailPanel.CurrentCharacter.ChangeHP(1);
        _hpLevelUpStatBar.SetBar(_detailPanel.CurrentCharacter.HitPoints);
        _detailPanel.CurrentCharacter.SpendStatPoint();

        CheckStatCapOrPointsSpent();
    }

    private void CheckStatCapOrPointsSpent()
    {
        bool hpAtMaxVal = _detailPanel.CurrentCharacter.HitPoints >= 8;
        bool charHasStatPoints = _detailPanel.CurrentCharacter.PendingStatPoints > 0;

        _increaseAccRating.gameObject.SetActive(charHasStatPoints);
        _increaseHP.gameObject.SetActive(!hpAtMaxVal && charHasStatPoints);

        _statPoints.text = "Stat Points: " + _detailPanel.CurrentCharacter.PendingStatPoints;

    }

    public void HandleLvlUpCardSelected(Ability a)
    {
        _detailPanel.CurrentCharacter.Abilities.Add(a);
        ApplyChanges();
    }

    private void ApplyChanges()
    {
        // refresh the detail panel w/ new stats
        //_detailPanel.SetCharacter(_detailPanel.CurrentCharacter);
        _decisionsManager.ShowCharacterPanel(_detailPanel.CurrentCharacter);
    }
}