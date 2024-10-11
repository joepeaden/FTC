using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfoLine : MonoBehaviour
{
    [SerializeField] TMP_Text infoLabel;
    [SerializeField] TMP_Text infoValue;

    public void SetData(string infoLabelString, string infoValueString)
    {
        infoLabel.text = infoLabelString;
        infoValue.text = infoValueString;
    }
}
