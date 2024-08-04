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
        transform.position = new Vector3(position.x, position.y + heightOffset, position.z);
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