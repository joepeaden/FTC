using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TroopPanel : MonoBehaviour
{
    [SerializeField] TMP_Text troopName;

    public void SetupPanel(CharInfo info)
    {
        troopName.text = info.CharName;
    }
}
