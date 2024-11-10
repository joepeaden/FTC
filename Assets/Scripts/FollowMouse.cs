using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    private RectTransform _rect;

    private void Start()
    {
        _rect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponent<RectTransform>().parent as RectTransform, 
            Input.mousePosition,
            CameraManager.MainCamera,
            out mousePos);

        // Update the position of the UI element
        _rect.anchoredPosition = mousePos;
    }
}
