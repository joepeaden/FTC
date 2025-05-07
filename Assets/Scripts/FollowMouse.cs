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

    private void Update()
    {
        if (ShouldFollow)
        {
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                GetComponent<RectTransform>().parent as RectTransform, 
                Input.mousePosition,
                null,
                out mousePos);

            // Update the position of the UI element
            _rect.anchoredPosition = mousePos + offset;
        }
    }
}
