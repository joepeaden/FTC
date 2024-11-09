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
            CameraManager.MainCamera,
            out mousePos);



        // Convert screen position to a position relative to the UI's canvas
        //Vector2 uiPos;
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(
        //    transform.parent as RectTransform,
        //    newPos,
        //    Camera.main,
        //    out uiPos);

        //GetComponent<RectTransform>().anchoredPosition = uiPos;

        // Update the position of the UI element
        GetComponent<RectTransform>().anchoredPosition = mousePos;
    }
}
