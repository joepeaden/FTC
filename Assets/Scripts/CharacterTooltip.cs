using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterTooltip : MonoBehaviour
{
    [SerializeField] private GameObject _tooltip;

    [SerializeField] TMP_Text characterNameText;
    [SerializeField] TMP_Text characterMotivatorText;
    [SerializeField] TMP_Text hitChanceText;
    [SerializeField] GameObject hitChanceUI;
    [SerializeField] StatBar armorBar;
    [SerializeField] StatBar healthBar;
    [SerializeField] StatBar apBar;
    [SerializeField] StatBar motBar;

    private Pawn _currentPawn;

    public void SetPawn(Pawn p)
    {
        _tooltip.SetActive(true);
        hitChanceUI.SetActive(false);
        characterNameText.text = p.GameChar.CharName;
        characterMotivatorText.text = p.CurrentMotivator.ToString();
        armorBar.SetBar(p.MaxArmorPoints, p.ArmorPoints);
        healthBar.SetBar(p.MaxHitPoints, p.HitPoints);
        apBar.SetBar(Pawn.BASE_ACTION_POINTS, p.ActionPoints);
        motBar.SetBar(p.MaxMotivation, p.Motivation);

        _currentPawn = p;
    }

    public void Hide()
    {
        hitChanceUI.SetActive(false);
        _tooltip.SetActive(false);
    }

    public void ShowHitPreview(float chance, int hpDamage, int arDamage)
    {
        hitChanceUI.SetActive(true);
        hitChanceText.text = "To Hit: " + Mathf.RoundToInt(chance * 100) + "%";

        healthBar.SetBar(_currentPawn.MaxHitPoints, _currentPawn.HitPoints, (_currentPawn.HitPoints - hpDamage));
        armorBar.SetBar(_currentPawn.MaxArmorPoints, _currentPawn.ArmorPoints, (_currentPawn.ArmorPoints - arDamage));
    }
}
