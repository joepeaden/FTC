using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine;

public class ContextButton : MonoBehaviour
{
    [SerializeField] private TMP_Text displayText;

    public void SetData(string text, UnityAction callback)
    {
        displayText.text = text;
        GetComponent<Button>().onClick.AddListener(() => { callback(); });
    }
}
