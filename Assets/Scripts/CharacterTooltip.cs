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

    //private void Start()
    //{
    //    BattleManager.Instance.OnActionUpdated.AddListener(UpdateTooltip);
    //}

    //private void OnDestroy()
    //{
    //    BattleManager.Instance.OnActionUpdated.RemoveListener(UpdateTooltip);
    //}

    //private void UpdateTooltip(ActionData newAction)
    //{
    //    if (_tooltip.activeInHierarchy)
    //    {
    //        SetPawn(_currentPawn);

    //        if (hitChanceUI.activeInHierarchy && newAction != null)
    //        {
    //            // cheating here, but whatever. If you're a potential employer
    //            // and you're looking at this - I wouldn't do this to you! But
    //            // I'm the boss here and I'm not paying myself, so I say it's okay.
    //            ShowHitPreview(int.Parse(hitChanceText.text), _currentPawn.GameChar.GetWeaponDamageForAction(newAction));
    //        }
    //    }
    //}

    public void SetPawn(Pawn p)
    {
        _tooltip.SetActive(true);
        hitChanceUI.SetActive(false);
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
