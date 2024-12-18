using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleLogEntry : MonoBehaviour
{
    [SerializeField] TMP_Text entry;

    public void SetText(string text)
    {
        entry.text = text;
    }
}
