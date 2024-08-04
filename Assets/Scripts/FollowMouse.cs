using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    private void Update()
    {
        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponent<RectTransform>().parent as RectTransform, 
            Input.mousePosition,
            null,
            out mousePos);

        // Update the position of the UI element
        GetComponent<RectTransform>().anchoredPosition = mousePos;
    }
}
