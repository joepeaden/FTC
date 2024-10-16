using System.Collections;
using UnityEngine;
using TMPro;

public class TextFloatUp : MonoBehaviour
{
    public float visibleTime;
    public float fadeIncrement;
    public float fadeSpeed;
    public TMP_Text textElement;

    [SerializeField] private float heightOffset;
    public bool InUse => _inUse;
    private bool _inUse;

    public void SetData(Vector3 position, string text, Color color)
    {
        StopAllCoroutines();

        Vector3 newPos = Camera.main.WorldToScreenPoint(position);
        newPos.y += heightOffset;

        // Convert screen position to a position relative to the UI's canvas
        Vector2 uiPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform,
            newPos,
            Camera.main,
            out uiPos);

        GetComponent<RectTransform>().anchoredPosition = uiPos;
        textElement.text = text;
        StartCoroutine(Fade());
        _inUse = true;
    }

    IEnumerator Fade()
    {
        textElement.alpha = 1;

        yield return new WaitForSeconds(visibleTime);

        for (float i = 1; i > 0; i -= fadeIncrement)
        {
            textElement.alpha = i;
            yield return new WaitForSecondsRealtime(fadeSpeed);
        }
        _inUse = false;
    }
}