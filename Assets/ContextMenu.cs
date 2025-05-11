using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ContextMenu : MonoBehaviour
{
    public static ContextMenu Instance => _instance;
    private static ContextMenu _instance;

    public static bool IsShowing; 

    [SerializeField] private ContextButton contextButtonPrefab;
    [SerializeField] private float _yOffset;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void SetOptionsAndShow(Tile tile, Dictionary<string, UnityAction> options)
    {
        ClearChildren();

        foreach (KeyValuePair<string, UnityAction> kvp in options)
        {
            string text = kvp.Key;
            UnityAction callback = kvp.Value;

            ContextButton newContextButton = Instantiate(contextButtonPrefab, transform);
            newContextButton.SetData(text, callback);
        }
        
        // set position at tile
        Vector3 newPos = CameraManager.MainCamera.WorldToScreenPoint(tile.transform.position);
        newPos.y += _yOffset;
        Vector2 uiPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform,
            newPos,
            null,
            out uiPos);
        GetComponent<RectTransform>().localPosition = uiPos;

        IsShowing = true;
    }

    private void ClearChildren()
    {
        // DESTROY THE CHILDREN MUAHAHAHAHAHAHAHAHAHAHAHAHAHAAHA
        
        // haiku time

        // Mercy can't be had —
        // Though pooling might be better,
        // Children must be crushed.
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    public void Hide()
    {        
        ClearChildren();

        IsShowing = false;
    }
}
