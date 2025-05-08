using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    public Vector2 offset;
    
    private RectTransform _rect;

    // Lol. Should the "followmouse" script follow the mouse? Not always, apparently.
    [HideInInspector] public bool ShouldFollow = true; 

    private void Start()
    {
        _rect = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Set to the mouse position + offset. Can be used to set position instantly without having it follow the 
    /// mouse every frame.
    /// </summary>
    public void SetAtMouse()
    {
        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rect.parent as RectTransform, 
            Input.mousePosition,
            null,
            out mousePos);

        // Update the position of the UI element
        _rect.anchoredPosition = mousePos + offset;
    }

    private void Update()
    {
        // if (ShouldFollow)
        // {
            SetAtMouse();
        // }
    }
}
