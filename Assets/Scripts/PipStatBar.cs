using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PipStatBar : MonoBehaviour
{
    [SerializeField] private Color fill;
    [SerializeField] private List<Image> statPips = new();
    [SerializeField] private TMP_Text statValueText;

    private void Awake()
    {
        foreach (Image pip in statPips)
        {
            pip.color = fill;
        }
    }

    /// <summary>
    /// Set the bar based on a total and current value
    /// </summary>
    /// <param name="totalStatValue"></param>
    /// <param name="currentValue"></param>
    public void SetBar(int currentValue)
    {
        if (currentValue > statPips.Count - 1)
        {
            return;
        }

        statValueText.text = currentValue.ToString();

        // set currentValue num of pips to active
        int i = 0;
        for (; i < currentValue; i++)
        {
            if (!statPips[i].gameObject.activeInHierarchy)
            {
                statPips[i].gameObject.SetActive(true);
            }
        }

        // set rest inactive
        for (; i < statPips.Count; i++)
        {
            if (statPips[i].gameObject.activeInHierarchy)
            {
                statPips[i].gameObject.SetActive(false);
            }
            // no need to set more inactive if we're already inactive here
            else
            {
                break;
            }
        }
    }
}