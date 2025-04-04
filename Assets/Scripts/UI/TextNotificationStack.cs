using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using System;
using UnityEngine.UIElements;

public class TextNotificationStack : MonoBehaviour
{
    public float visibleTime;
    public float fadeIncrement;
    public float fadeSpeed;
    public float moveSpeed;

    [SerializeField] private float yOffset;
    public bool InUse => _inUse;
    private bool _inUse;

    List<TMP_Text> textElements = new();

    private void Awake()
    {
        // add all children to list - these are the individual text objects
        for (int i = 0; i < transform.childCount; i++)
        {
            textElements.Add(transform.GetChild(i).GetComponent<TMP_Text>());
        }   

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_inUse)
        {
            transform.Translate(Vector2.up * Time.deltaTime * moveSpeed);
        }        
    }

    public void SetData(Vector3 position, List<(string, Color)> notifs)
    {
        StopAllCoroutines();

        gameObject.SetActive(true);

        // Set Position of notification
        Vector3 newPos = CameraManager.MainCamera.WorldToScreenPoint(position);
        newPos.y += yOffset;
        Vector2 uiPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform,
            newPos,
            CameraManager.MainCamera,
            out uiPos);
        GetComponent<RectTransform>().localPosition = uiPos;


        int i = 0;
        // set colors and text strings
        foreach ((string, Color) notif in notifs)
        {
            textElements[i].text = notif.Item1;
            textElements[i].color = notif.Item2;
            // reset transparency <- is that how you spell it? idk.
            // I just had a chocolate muffin. Last night I ate like a whole large pizza
            // oh my goodness.
            textElements[i].alpha = 1;
            i++;
        }

        for (; i < textElements.Count; i++)
        {
            textElements[i].text = "";
        }
        
        StartCoroutine(TimeOut());
        _inUse = true;
    }


    IEnumerator TimeOut()
    {
        yield return new WaitForSeconds(visibleTime);

        // OPTIMIZE: Potential issue, iterating each element in the list every iteration here.
        for (float i = 1; i > 0; i -= fadeIncrement)
        {
            foreach (TMP_Text txt in textElements)
            {
                if (txt.text != "")
                {
                    txt.alpha = i;       
                }
            }

            yield return new WaitForSecondsRealtime(fadeSpeed);
        }

        _inUse = false;
    }
}