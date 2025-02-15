using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// An ungodly amalgamation of different tooltips, brought into one being by eldritch rituals
/// </summary>
public class EquipmentTooltip : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text descriptionText;
    [SerializeField] GameObject headerDiv;

    [SerializeField] GameObject infoLine;
    [SerializeField] Transform infoParent;

    private List<InfoLine> _infoLines = new();

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        for (int i = 0; i < infoParent.childCount; i++)
        {
            _infoLines.Add(infoParent.GetChild(i).gameObject.GetComponent<InfoLine>());
        } 
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

        HideInfoLines();

        if (item.itemType == ItemType.Weapon)
        {
            WeaponItemData weaponItem = (WeaponItemData)item;
            SetLine("Damage", weaponItem.baseDamage.ToString());
        }
        else
        {
            ArmorItemData armorItem = (ArmorItemData)item;
            SetLine("Armor", armorItem.protection.ToString());
            SetLine("Move", armorItem.moveMod.ToString());
            SetLine("Initiative", armorItem.initMod.ToString());
            
            //UpdateModText(viceModElements, viceModValue, armorItem.viceMod);
            //if (armorItem.viceMod != 0)
            //{
            //    switch (armorItem.viceToMod)
            //    {
            //        case GameCharacter.CharMotivators.Greed:
            //            viceModLabel.text = "GRD:";
            //            break;
            //        case GameCharacter.CharMotivators.Honor:
            //            viceModLabel.text = "HNR:";
            //            break;
            //        case GameCharacter.CharMotivators.Glory:
            //            viceModLabel.text = "GLY:";
            //            break;
            //    }
            //}
        }
    }

    public void SetAction(AbilityData ability)
    {
        if (ability == null)
        {
            return;
        }

        gameObject.SetActive(true);
        headerDiv.SetActive(true);

        nameText.text = ability.abilityName;
        descriptionText.text = ability.description;

        HideInfoLines();

        SetLine("MOT Cost", ability.motCost.ToString());

        WeaponAbilityData weaponAbility = ability as WeaponAbilityData;
        if (weaponAbility)
        {
            SetLine("AP Cost", weaponAbility.apCost.ToString());
            SetLine("Range", ability.range.ToString());

            if (weaponAbility.bonusDmg > 0)
            {
                SetLine("Damage Boost", "+" + weaponAbility.bonusDmg.ToString());
            }

            if (weaponAbility.critChanceMod > 0)
            {
                SetLine("Crit Roll Bonus", (weaponAbility.critChanceMod).ToString());
            }
        }

        SupportAbilityData supportAbility = ability as SupportAbilityData;
        if (supportAbility)
        {
            SetLine("Range", ability.range.ToString());

            if (supportAbility.outDmgMod > 0)
            {
                SetLine("Damage Boost", supportAbility.outDmgMod.ToString());
            }
            if (supportAbility.inDmgMod > 0)
            {
                SetLine("Frailty", supportAbility.inDmgMod.ToString());
            }
            if (supportAbility.hitMod > 0)
            {
                SetLine("Hit Boost", "+" + supportAbility.hitMod);
            }
            if (supportAbility.dodgeMod > 0)
            {
                SetLine("Dodge Boost", supportAbility.dodgeMod.ToString());
            }
        }
    }

    public void SetEffect(EffectData effect)
    {
        if (effect == null)
        {
            return;
        }

        gameObject.SetActive(true);
        headerDiv.SetActive(false);

        nameText.text = effect.effectName;
        descriptionText.text = effect.description;

        HideInfoLines();
    }

    public void SetDescription(string title, string description)
    {
        gameObject.SetActive(true);
        headerDiv.SetActive(false);

        nameText.text = title;
        descriptionText.text = description;

        HideInfoLines();
    }

    private void HideInfoLines()
    {
        foreach (InfoLine line in _infoLines)
        {
            line.Hide();
        }
    }

    private void SetLine(string label, string value)
    {
        if (int.TryParse(value, out int result))
        {
            // don't show zero values (negative values probably should be shown)
            if (result == 0)
            {
                return;
            }
        }
        
        foreach (InfoLine line in _infoLines)
        {
            if (line.isHidden)
            {
                line.SetData(label, value);
                return;
            }
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
