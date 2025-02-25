using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PostBattlePawnRow : MonoBehaviour
{
    [SerializeField] private PawnPreview _pawnPreview;
    [SerializeField] private TMP_Text _charName;
    [SerializeField] private TMP_Text _kills;
    [SerializeField] private TMP_Text _damageInflicted;
    [SerializeField] private TMP_Text _xp;
    [SerializeField] private List<EffectIcon> _effectIcons;

    public void SetData(Pawn p)
    {
        gameObject.SetActive(true);

        _pawnPreview.SetData(p.GameChar);

        _charName.text = p.GameChar.CharName;
        _kills.text = "Kills: " + p.BattleKills.ToString();
        _damageInflicted.text = "Damage Inflicted: " + p.DmgInflicted.ToString();
        _xp.text = "XP: " + p.GameChar.XP + "/" + p.GameChar.GetXPToLevel();

        int i = 0;
        foreach (EffectData effect in p.GameChar.Effects)
        {
            if (i >= _effectIcons.Count)
            {
                Debug.Log("ALERT: Too many effects, not enough icons.");
                break;
            }

            _effectIcons[i].SetData(effect);
            i++;
        }

        for (; i < _effectIcons.Count; i++)
        {
            _effectIcons[i].Hide();
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
