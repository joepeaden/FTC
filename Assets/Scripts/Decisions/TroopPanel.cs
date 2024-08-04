using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TroopPanel : MonoBehaviour
{
    [SerializeField] TMP_Text troopNameText;

    public void SetupPanel(GameCharacter gameChar)
    {
        troopNameText.text = gameChar.CharName;
    }
}
