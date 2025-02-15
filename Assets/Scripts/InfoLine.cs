using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfoLine : MonoBehaviour
{
    [SerializeField] TMP_Text infoLabel;
    [SerializeField] TMP_Text infoValue;

    public bool isHidden = true;

    public void Hide()
    {
        isHidden = true;
        gameObject.SetActive(false);
    }

    public void SetData(string infoLabelString, string infoValueString)
    {
        isHidden = false;

        gameObject.SetActive(true);
        
        infoLabel.text = infoLabelString;
        infoValue.text = infoValueString;
    }
}
