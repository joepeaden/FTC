using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharDetailPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text _charName;
    [SerializeField] private TMP_Text _healthText;
    [SerializeField] private StatBar _healthBar;
    //[SerializeField] private TMP_Text _apText;
    //[SerializeField] private StatBar _apBar;
    //[SerializeField] private TMP_Text _motText;
    //[SerializeField] private StatBar _motBar;
    [SerializeField] private TMP_Text _sanctText;
    [SerializeField] private StatBar _sanctBar;
    [SerializeField] private TMP_Text _avaText;
    [SerializeField] private StatBar _avaBar;
    [SerializeField] private TMP_Text _vainText;
    [SerializeField] private StatBar _vainBar;
    //[SerializeField] private Transform _equippedHelmParent;
    //[SerializeField] private Transform _equippedWeaponParent;
    [SerializeField] private GameObject itemUIPrefab;

    private GameCharacter _currentCharacter;
    private DecisionsManager _decisions;
    [SerializeField] private ItemUI _helmUI;
    [SerializeField] private ItemUI _weaponUI;

    public void Setup(DecisionsManager decisions)
    {
        _decisions = decisions;
    }

    public void SetCharacter(GameCharacter character)
    {
        _currentCharacter = character;
        _charName.text = character.CharName;
        _healthText.text = "Hit Points: " + character.HitPoints;
        _healthBar.SetBar(character.HitPoints, character.HitPoints);
        _sanctText.text = "Sanctimony: " + character.Sanctimony;
        _sanctBar.SetBar(10, character.Sanctimony);
        _avaText.text = "Avarice: " + character.Avarice;
        _avaBar.SetBar(10, character.Avarice);
        _vainText.text = "Vainglory: " + character.Vainglory;
        _vainBar.SetBar(10, character.Vainglory);

        _helmUI.RemoveCallbacks();
        _weaponUI.RemoveCallbacks();

        if (character.HelmItem != null)
        {
            //if (_helmUI == null)
            //{
            //    _helmUI = Instantiate(itemUIPrefab, _equippedHelmParent).GetComponent<ItemUI>();
            //}

            _helmUI.SetData(character.HelmItem, _decisions, UnEquipItem);
        }
        else if (_helmUI != null)
        {
            _helmUI.Hide();
            //Destroy(_helmUI.gameObject);
        }

        if (character.WeaponItem != null)
        {
            //if (_weaponUI == null)
            //{
            //    _weaponUI = Instantiate(itemUIPrefab, _equippedWeaponParent).GetComponent<ItemUI>();
            //}

            _weaponUI.SetData(character.WeaponItem, _decisions, UnEquipItem);
        }
        else if (_weaponUI != null)
        {
            _weaponUI.Hide();
            //Destroy(_weaponUI.gameObject);
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
