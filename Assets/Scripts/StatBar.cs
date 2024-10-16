using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatBar : MonoBehaviour
{
    public GameObject statPipPrefab;
    public Transform statBarParent;
    [SerializeField] private Color fill;
    [SerializeField] private Color bgFill;
    private Color _previewFill;

    private List<Image> _previewPips = new();

    //private bool _previewGreaterThan;

    private void Start()
    {
        //StartCoroutine(FlashPreviewPips());

        // make the preview background gray
        Color prevFill = fill;
        prevFill.r -= .5f;
        prevFill.g -= .5f;
        prevFill.b -= .5f;
        _previewFill = prevFill;
    }

    /// <summary>
    /// Set the bar based on a total and current value
    /// </summary>
    /// <param name="totalStatValue"></param>
    /// <param name="currentValue"></param>
    public void SetBar(int totalStatValue, int currentValue, int previewValue = -1)
    {
        _previewPips.Clear();

        if (totalStatValue != statBarParent.transform.childCount)
        {
            for (int i = 0; i < statBarParent.transform.childCount; i++)
            {
                Destroy(statBarParent.GetChild(i).gameObject);
            }

            for (int i = 0; i < totalStatValue; i++)
            {
                SetStatPipColor(Instantiate(statPipPrefab, statBarParent), currentValue, previewValue, i);
            }
        }
        else
        {
            for (int i = 0; i < totalStatValue; i++)
            {
                SetStatPipColor(statBarParent.transform.GetChild(i).gameObject, currentValue, previewValue, i);
            }
        }
    }

    private void SetStatPipColor(GameObject statPip, int currentVal, int previewVal, int index)
    {
        Image pipImage = statPip.GetComponent<Image>();
        //if (currentVal > previewVal)
        //{
            //_previewGreaterThan = false;
            if (currentVal <= index)
            {
                if (_previewPips.Contains(pipImage))
                {
                    _previewPips.Remove(pipImage);
                }

                pipImage.color = bgFill;


            }
            // previewValue will be -1 if no preview requested
            else if (previewVal != -1 && previewVal <= index)
            {
                if (!_previewPips.Contains(pipImage))
                {
                    pipImage.color = _previewFill;
                    //_previewPips.Add(pipImage);
                }
            }
            else
            {
                if (_previewPips.Contains(pipImage))
                {
                    _previewPips.Remove(pipImage);
                }

                pipImage.color = fill;
            }
        //}
        //else
        //{
        //    _previewGreaterThan = true;

        //    // previewValue will be -1 if no preview requested
        //    if (previewVal <= index)
        //    {
        //        if (_previewPips.Contains(pipImage))
        //        {
        //            _previewPips.Remove(pipImage);
        //        }

        //        pipImage.color = bgFill;
                
        //    }
        //    else if(previewVal != -1 && currentVal <= index)
        //    {
        //        if (!_previewPips.Contains(pipImage))
        //        {
        //            pipImage.color = _previewFill;
        //            //_previewPips.Add(pipImage);
        //        }
                
        //    }
        //    else
        //    {
        //        if (_previewPips.Contains(pipImage))
        //        {
        //            _previewPips.Remove(pipImage);
        //        }

        //        pipImage.color = fill;
                
        //    }
        //}
    }

    //private IEnumerator FlashPreviewPips()
    //{
    //    while (true)
    //    {
    //        Debug.Log(_previewPips.Count);

    //        if (_previewGreaterThan)
    //        {
    //            for (float i = 0; i < 1; i += .01f)
    //            {
    //                foreach (Image pipImage in _previewPips)
    //                {
    //                    Color pipColor = pipImage.color;
    //                    pipColor.a = i;
    //                    pipImage.color = pipColor;
    //                }
    //                yield return new WaitForSeconds(.001f);
    //            }

    //            for (float i = 1; i > 0; i -= .01f)
    //            {
    //                foreach (Image pipImage in _previewPips)
    //                {
    //                    Color pipColor = pipImage.color;
    //                    pipColor.a = i;
    //                    pipImage.color = pipColor;
    //                }
    //                yield return new WaitForSeconds(.001f);
    //            }
    //        }
    //        else
    //        {
    //            for (float i = 1; i > 0; i -= .01f)
    //            {
    //                foreach (Image pipImage in _previewPips)
    //                {
    //                    Color pipColor = pipImage.color;
    //                    pipColor.a = i;
    //                    pipImage.color = pipColor;
    //                }
    //                yield return new WaitForSeconds(.001f);
    //            }


    //            for (float i = 0; i < 1; i += .01f)
    //            {
    //                foreach (Image pipImage in _previewPips)
    //                {
    //                    Color pipColor = pipImage.color;
    //                    pipColor.a = i;
    //                    pipImage.color = pipColor;
    //                }
    //                yield return new WaitForSeconds(.001f);
    //            }
    //        }
    //    }
    //}
}