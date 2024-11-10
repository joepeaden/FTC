using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatBar : MonoBehaviour
{
    public GameObject statPipPrefab;
    public RectTransform statBarParent;
    [SerializeField] private Color fill;
    [SerializeField] private Color bgFill;
    private Color _previewFill;

    private List<Image> _previewPips = new();

    private List<Image> statPips = new();

    [SerializeField] private RectTransform fillBar;

    private void Awake()
    {
        //StartCoroutine(FlashPreviewPips());

        // make the preview background gray
        Color prevFill = fill;
        prevFill.r -= .5f;
        prevFill.g -= .5f;
        prevFill.b -= .5f;
        _previewFill = prevFill;

        fillBar.GetComponent<Image>().color = fill;
    }


    /// <summary>
    /// Set the bar based on a total and current value
    /// </summary>
    /// <param name="totalStatValue"></param>
    /// <param name="currentValue"></param>
    public void SetBar(int totalStatValue, int currentValue, int previewValue = -1)
    {
        if (totalStatValue == 0)
        {
            return;
        }

        float parentWidth = statBarParent.rect.width;

        // Calculate target width based on the desired ratio
        float targetRatio = (float)currentValue / (float)totalStatValue;
        float targetWidth = parentWidth * targetRatio;

        // Set the width of _rect by adjusting its sizeDelta
        fillBar.sizeDelta = new Vector2(targetWidth, fillBar.sizeDelta.y);

        //int i = 0;
        //if (statPips.Count == 0)
        //{
        //    // add... a lot of them for now.
        //    for (; i < 300; i++)
        //    {
        //        statPips.Add(Instantiate(statPipPrefab, statBarParent).GetComponent<Image>());
        //        statPips[i].gameObject.SetActive(false);
        //    }
        //}

        //_previewPips.Clear();

        //i = 0;
        //for (; i < totalStatValue; i++)
        //{
        //    Image statPip = statPips[i];
        //    if (!statPip.gameObject.activeInHierarchy)
        //    {
        //        statPip.gameObject.SetActive(true);
        //    }

        //    SetStatPipColor(statPip, currentValue, previewValue, i);
        //}

        //for (; i < statPips.Count; i++)
        //{
        //    if (statPips[i].gameObject.activeInHierarchy)
        //    {
        //        statPips[i].gameObject.SetActive(false);
        //    }
        //}
    }

    private void SetStatPipColor(Image statPip, int currentVal, int previewVal, int index)
    {

        if (currentVal <= index)
        {
            if (_previewPips.Contains(statPip))
            {
                _previewPips.Remove(statPip);
            }

            if (statPip.color != bgFill)
            {
                statPip.color = bgFill;
            }

        }
        // previewValue will be -1 if no preview requested
        else if (previewVal != -1 && previewVal <= index)
        {
            if (!_previewPips.Contains(statPip) && statPip.color != _previewFill)
            {
                statPip.color = _previewFill;
            }
        }
        else
        {
            if (_previewPips.Contains(statPip))
            {
                _previewPips.Remove(statPip);
            }

            if (statPip.color != fill)
            {
                statPip.color = fill;
            }
        }   
    }
}