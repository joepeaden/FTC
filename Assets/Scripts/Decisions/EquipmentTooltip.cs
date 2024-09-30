using UnityEngine;
using TMPro;

public class EquipmentTooltip : MonoBehaviour
{
    [SerializeField] TMP_Text equipmentNameText;
    [SerializeField] TMP_Text descriptionText;

    [SerializeField] GameObject armorElements;
    [SerializeField] TMP_Text protValue;
    [SerializeField] GameObject moveModElements;
    [SerializeField] TMP_Text moveModValue;
    [SerializeField] GameObject initModElements;
    [SerializeField] TMP_Text initModValue;
    [SerializeField] GameObject viceModElements;
    [SerializeField] TMP_Text viceModLabel;
    [SerializeField] TMP_Text viceModValue;

    [SerializeField] GameObject weaponElements;
    [SerializeField] TMP_Text dmgValue;

    private ItemData _currentItem;

    public void SetItem(ItemData item)
    {
        if (item == null)
        {
            return;
        }

        gameObject.SetActive(true);

        equipmentNameText.text = item.itemName;
        descriptionText.text = item.description;

        if (item.itemType == ItemType.Weapon)
        {
            weaponElements.SetActive(true);
            armorElements.SetActive(false);
            WeaponItemData weaponItem = (WeaponItemData)item;
            dmgValue.text = weaponItem.damage.ToString();
        }
        else
        {
            weaponElements.SetActive(false);
            armorElements.SetActive(true);
            ArmorItemData armorItem = (ArmorItemData)item;
            protValue.text = armorItem.protection.ToString();

            UpdateModText(moveModElements, moveModValue, armorItem.moveMod);
            UpdateModText(initModElements, initModValue, armorItem.initMod);
            UpdateModText(viceModElements, viceModValue, armorItem.viceMod);
            if (armorItem.viceMod != 0)
            {
                switch (armorItem.viceToMod)
                {
                    case GameCharacter.CharVices.Greed:
                        viceModLabel.text = "GRD:";
                        break;
                    case GameCharacter.CharVices.Honor:
                        viceModLabel.text = "HNR:";
                        break;
                    case GameCharacter.CharVices.Glory:
                        viceModLabel.text = "GLY:";
                        break;
                }
            }
        }

        _currentItem = item;
    }

    private void UpdateModText(GameObject parentGO, TMP_Text valueText, int modifierValue)
    {
        if (modifierValue == 0)
        {
            valueText.gameObject.SetActive(false);
            parentGO.gameObject.SetActive(false);
        }
        else
        {
            valueText.gameObject.SetActive(true);
            parentGO.gameObject.SetActive(true);
            valueText.text = modifierValue > 0 ? "+" + modifierValue.ToString() : modifierValue.ToString();
        }
    }    

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
