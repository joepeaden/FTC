using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class CharDetailPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text _charName;
    [SerializeField] private TMP_Text _healthText;
    [SerializeField] private TMP_Text _viceText;
    [SerializeField] private TMP_Text _motivText;
    [SerializeField] private TMP_Text _moveText;
    [SerializeField] private TMP_Text _initText;
    [SerializeField] private GameObject itemUIPrefab;

    private GameCharacter _currentCharacter;
    private DecisionsManager _decisions;
    [SerializeField] private ItemUI _helmUI;
    [SerializeField] private ItemUI _weaponUI;
    [SerializeField] private List<ActionButton> _actionButtons = new();

    public void Setup(DecisionsManager decisions)
    {
        _decisions = decisions;
    }

    public void SetCharacter(GameCharacter character)
    {
        _currentCharacter = character;
        _charName.text = _currentCharacter.CharName;
        _healthText.text = _currentCharacter.HitPoints.ToString();
        _viceText.text = _currentCharacter.Motivator.ToString() + " (" + _currentCharacter.GetTotalViceValue().ToString() + ")";
        _motivText.text = _currentCharacter.GetBattleMotivationCap().ToString();
        _moveText.text = _currentCharacter.GetMoveRange().ToString();
        _initText.text = _currentCharacter.GetInitiativeWithEquipment().ToString();

        _helmUI.RemoveCallbacks();
        _weaponUI.RemoveCallbacks();

        if (character.HelmItem != null)
        {
            _helmUI.SetData(character.HelmItem, _decisions, UnEquipItem);
        }
        else if (_helmUI != null)
        {
            _helmUI.Hide();
        }

        if (character.TheWeapon != null)
        {
            _weaponUI.SetData(character.TheWeapon.Data, _decisions, UnEquipItem);
        }
        else if (_weaponUI != null)
        {
            _weaponUI.Hide();
        }

        List<Ability> pawnAbilities = character.GetAbilities();
        // there's currently only 4 ability buttons - will need to address that at some point,
        // could cause problems.
        int i = 0;
        for (; i < pawnAbilities.Count; i++)
        {
            ActionButton actionButton = _actionButtons[i];

            actionButton.SetSelected(false);
            actionButton.gameObject.SetActive(true);

            if (actionButton.TheAbility != pawnAbilities[i])
            {
                actionButton.SetDataDisplay(pawnAbilities[i]);
            }
        }

        // update the remaining buttons
        for (; i < _actionButtons.Count; i++)
        {
            ActionButton actionButton = _actionButtons[i];
            actionButton.SetSelected(false);
            actionButton.gameObject.SetActive(false);
        }
    }

    public void UnEquipItem(ItemUI itemUI)
    {
        // don't allow unequipping of default item
        if (itemUI.Item.isDefault)
        {
            return;
        }

        _currentCharacter.UnEquipItem(itemUI.Item);
        GameManager.Instance.PlayerInventory.Add(itemUI.Item);

        switch (itemUI.Item.itemType)
        {
            case ItemType.Helmet:
                _helmUI.Clear();
                break;
            case ItemType.Weapon:
                _weaponUI.Clear();
                break;
        }

        _decisions.RefreshInventory();

        SetCharacter(_currentCharacter);
    }

    public void EquipItem(ItemData item)
    {
        ItemData oldItem = _currentCharacter.EquipItem(item);

        if (oldItem != null)
        {
            // put the item back in player inventory if it's not the default one
            if (!oldItem.isDefault)
            {
                GameManager.Instance.PlayerInventory.Add(oldItem);
            }
        }

        SetCharacter(_currentCharacter);
    }
}
