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
                    case GameCharacter.CharMotivators.Greed:
                        viceModLabel.text = "GRD:";
                        break;
                    case GameCharacter.CharMotivators.Honor:
                        viceModLabel.text = "HNR:";
                        break;
                    case GameCharacter.CharMotivators.Glory:
                        viceModLabel.text = "GLY:";
                        break;
                }
            }
        }
    }

    public void SetAction(AbilityData ability)
    {
        if (ability == null)
        {
            return;
        }

        // eventually I gotta remove all this ActionData crap. But I don't have time to rework even more right now.
        ActionData action = ability as ActionData;

        armorElements.SetActive(false);
        weaponElements.SetActive(false);

        gameObject.SetActive(true);
        headerDiv.SetActive(true);

        nameText.text = ability.abilityName;
        descriptionText.text = ability.description;

        for (int i = 0; i < infoParent.childCount; i++)
        {
            Destroy(infoParent.GetChild(i).gameObject);
        }

        GameObject newLine = Instantiate(infoLine, infoParent);
        newLine.GetComponent<InfoLine>().SetData("MOT Cost", ability.cost.ToString());

        if (action)
        {
            newLine = Instantiate(infoLine, infoParent);
            newLine.GetComponent<InfoLine>().SetData("AP Cost", action.apCost.ToString());
        }

        newLine = Instantiate(infoLine, infoParent);
        newLine.GetComponent<InfoLine>().SetData("Range", ability.range.ToString());

        if (ability.outDmgMod > 0)
        {
            newLine = Instantiate(infoLine, infoParent);
            newLine.GetComponent<InfoLine>().SetData("DMG Inflicted Mod", ability.outDmgMod.ToString());
        }

        if (ability.inDmgMod > 0)
        {
            newLine = Instantiate(infoLine, infoParent);
            newLine.GetComponent<InfoLine>().SetData("DMG Taken Mod", ability.inDmgMod.ToString());
        }

        if (action != null && action.armorDamageMod > 0)
        {
            newLine = Instantiate(infoLine, infoParent);
            newLine.GetComponent<InfoLine>().SetData("AMR DMG Mod", (action.armorDamageMod * 100).ToString() + "%");
        }

        if (action != null && action.penetrationDamageMod > 0)
        {
            newLine = Instantiate(infoLine, infoParent);
            newLine.GetComponent<InfoLine>().SetData("AMR PEN Mod", (action.penetrationDamageMod * 100).ToString() + "%");
        }

        if (ability.hitMod > 0)
        {
            newLine = Instantiate(infoLine, infoParent);
            newLine.GetComponent<InfoLine>().SetData("Hit Mod", (ability.hitMod * 100).ToString() + "%");
        }

        if (ability.dodgeMod > 0)
        {
            newLine = Instantiate(infoLine, infoParent);
            newLine.GetComponent<InfoLine>().SetData("Dodge Mod", (ability.dodgeMod * 100).ToString() + "%");
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
