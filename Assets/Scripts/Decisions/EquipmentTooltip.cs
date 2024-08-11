using UnityEngine;
using TMPro;

public class EquipmentTooltip : MonoBehaviour
{
    [SerializeField] TMP_Text equipmentNameText;
    [SerializeField] TMP_Text descriptionText;
    [SerializeField] TMP_Text statText;
    [SerializeField] StatBar statBar;

    private ItemData _currentItem;

    public void SetItem(ItemData item)
    {
        gameObject.SetActive(true);

        equipmentNameText.text = item.itemName;
        descriptionText.text = item.description;

        if (item.itemType == ItemType.Weapon)
        {
            WeaponItemData weaponItem = (WeaponItemData)item;
            statBar.SetBar(WeaponItemData.MAX_WEAPON_DAMAGE, weaponItem.damage);
            statText.text = "DMG: " + weaponItem.damage;
        }
        else
        {
            ArmorItemData armorItem = (ArmorItemData)item;
            statBar.SetBar(ArmorItemData.MAX_ARMOR_PROTECTION, armorItem.protection);
            statText.text = "PROT: " + armorItem.protection;
        }

        _currentItem = item;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
