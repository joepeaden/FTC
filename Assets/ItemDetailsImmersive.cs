using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// Handles character detail view and also the level up view
/// </summary>
public class ItemDetailsImmersive : MonoBehaviour
{
    [SerializeField] private TMP_Text _name;

    [SerializeField] private TMP_Text _cost;
    [SerializeField] private GameObject _weaponDetails;
    [SerializeField] private GameObject _armorDetails;
    [SerializeField] private TMP_Text _dmgText;
    [SerializeField] private TMP_Text _armorText;
    [SerializeField] private Image _itemSprite;
    [SerializeField] private List<ActionButton> _actionButtons = new();
    private ItemData _item;

    public void SetItem(ItemData item)
    {
        _item = item;
        _name.text = _item.itemName;
        _cost.text = "Cost: " + _item.itemPrice;
        _itemSprite.sprite = _item.itemSprite;
         List<Ability> itemAbilities = new();

        WeaponItemData weaponItem = _item as WeaponItemData;
        ArmorItemData armorItem = _item as ArmorItemData;
        if (weaponItem != null)
        {
            _weaponDetails.SetActive(true);
            _armorDetails.SetActive(false);
            _dmgText.text = weaponItem.baseDamage.ToString();
            itemAbilities = weaponItem.abilities;
        } 
        else if (armorItem != null)
        {
            _armorDetails.SetActive(true);
            _weaponDetails.SetActive(false);
            _armorText.text = armorItem.protection.ToString();
        }
        
        // there's currently only 4 ability buttons - will need to address that at some point,
        // could cause problems.
        int i = 0;
        for (; i < itemAbilities.Count; i++)
        {
            ActionButton actionButton = _actionButtons[i];

            actionButton.SetSelected(false);
            actionButton.gameObject.SetActive(true);

            if (actionButton.TheAbility != itemAbilities[i])
            {
                actionButton.SetDataDisplay(itemAbilities[i]);
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
}
