using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterTooltip : MonoBehaviour
{
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
        gameObject.SetActive(true);
        characterNameText.text = p.GameChar.CharName;
        characterMotivatorText.text = p.CurrentVice.ToString();
        armorBar.SetBar(p.MaxArmorPoints, p.ArmorPoints);
        healthBar.SetBar(p.MaxHitPoints, p.HitPoints);
        apBar.SetBar(Pawn.BASE_ACTION_POINTS, p.ActionPoints);
        motBar.SetBar(p.MaxMotivation, p.Motivation);

        _currentPawn = p;
    }

    public void Hide()
    {
        hitChanceUI.SetActive(false);
        gameObject.SetActive(false);
    }

    public void ShowHitPreview(float chance, int damage)
    {
        hitChanceUI.SetActive(true);
        hitChanceText.text = "To Hit: " + chance * 100 + "%";

        //healthBar.SetBar(_currentPawn.MaxHitPoints, _currentPawn.HitPoints, (_currentPawn.HitPoints - damage));
    }
}
