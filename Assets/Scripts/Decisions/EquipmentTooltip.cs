using UnityEngine;
using TMPro;

public class EquipmentTooltip : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text descriptionText;
    [SerializeField] GameObject headerDiv;

    [SerializeField] GameObject armorElements;
    [SerializeField] TMP_Text protValue;
    [SerializeField] GameObject moveModElements;
    [SerializeField] TMP_Text moveModValue;
    [SerializeField] GameObject initModElements;
    [SerializeField] TMP_Text initModValue;
    [SerializeField] GameObject viceModElements;
    [SerializeField] TMP_Text viceModLabel;
    [SerializeField] TMP_Text viceModValue;
    [SerializeField] TMP_Text armorDmgValue;
    [SerializeField] TMP_Text penDmgValue;
    [SerializeField] GameObject infoLine;
    [SerializeField] Transform infoParent;

    [SerializeField] GameObject weaponElements;
    [SerializeField] TMP_Text dmgValue;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void SetItem(ItemData item)
    {
        if (item == null)
        {
            return;
        }

        gameObject.SetActive(true);
        headerDiv.SetActive(true);

        nameText.text = item.itemName;
        descriptionText.text = item.description;

        for (int i = 0; i < infoParent.childCount; i++)
        {
            Destroy(infoParent.GetChild(i).gameObject);
        }

        if (item.itemType == ItemType.Weapon)
        {
            weaponElements.SetActive(true);
            armorElements.SetActive(false);
            WeaponItemData weaponItem = (WeaponItemData)item;
            dmgValue.text = weaponItem.baseDamage.ToString();
            armorDmgValue.text = (weaponItem.baseArmorDamage * 100).ToString() + "%";
            penDmgValue.text = (weaponItem.basePenetrationDamage * 100).ToString() + "%";
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
    }

    public void SetAction(ActionData action)
    {
        if (action == null)
        {
            return;
        }

        armorElements.SetActive(false);
        weaponElements.SetActive(false);

        gameObject.SetActive(true);
        headerDiv.SetActive(true);

        nameText.text = action.actionName;
        descriptionText.text = action.description;

        for (int i = 0; i < infoParent.childCount; i++)
        {
            Destroy(infoParent.GetChild(i).gameObject);
        }

        GameObject newLine = Instantiate(infoLine, infoParent);
        newLine.GetComponent<InfoLine>().SetData("MOT Cost", action.motCost.ToString());
        newLine = Instantiate(infoLine, infoParent);
        newLine.GetComponent<InfoLine>().SetData("AP Cost", action.apCost.ToString());
        newLine = Instantiate(infoLine, infoParent);
        newLine.GetComponent<InfoLine>().SetData("Range", action.range.ToString());

        if (action.damageMod > 0)
        {
            newLine = Instantiate(infoLine, infoParent);
            newLine.GetComponent<InfoLine>().SetData("DMG Mod", action.damageMod.ToString());
        }

        if (action.armorDamageMod > 0)
        {
            newLine = Instantiate(infoLine, infoParent);
            newLine.GetComponent<InfoLine>().SetData("AMR DMG Mod", (action.armorDamageMod * 100).ToString() + "%");
        }

        if (action.penetrationDamageMod > 0)
        {
            newLine = Instantiate(infoLine, infoParent);
            newLine.GetComponent<InfoLine>().SetData("AMR PEN Mod", (action.penetrationDamageMod * 100).ToString() + "%");
        }

        if (action.accMod > 0)
        {
            newLine = Instantiate(infoLine, infoParent);
            newLine.GetComponent<InfoLine>().SetData("ACC Mod", (action.accMod * 100).ToString() + "%");
        }
    }

    public void SetEffect(EffectData effect)
    {
        if (effect == null)
        {
            return;
        }

        armorElements.SetActive(false);
        weaponElements.SetActive(false);

        gameObject.SetActive(true);
        headerDiv.SetActive(false);

        nameText.text = effect.effectName;
        descriptionText.text = effect.description;

        for (int i = 0; i < infoParent.childCount; i++)
        {
            Destroy(infoParent.GetChild(i).gameObject);
        }
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
