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
    [SerializeField] private Transform _equippedHelmParent;
    [SerializeField] private Transform _equippedWeaponParent;
    [SerializeField] private GameObject itemUIPrefab;

    private GameCharacter _currentCharacter;
    private DecisionsManager _decisions;
    private ItemUI _helmUI;
    private ItemUI _weaponUI;

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

        _helmUI = Instantiate(itemUIPrefab, _equippedHelmParent).GetComponent<ItemUI>();
        _helmUI.SetData(character.HeadItem, _decisions, UnEquipItem);

        _weaponUI = Instantiate(itemUIPrefab, _equippedWeaponParent).GetComponent<ItemUI>();
        _weaponUI.SetData(character.WeaponItem, _decisions, UnEquipItem);
    }

    public void UnEquipItem(ItemUI itemUI)
    {
        switch (itemUI.Item.itemType)
        {
            case ItemType.Helmet:
                _helmUI.Clear();
                break;
            case ItemType.Weapon:
                _weaponUI.Clear();
                break;
        }

        _currentCharacter.UnEquipItem(itemUI.Item);
        _decisions.RefreshInventory();
    }

    public void EquipItem(ItemData item)
    {
        _currentCharacter.EquipItem(item);
        _decisions.RefreshInventory();
    }
}
