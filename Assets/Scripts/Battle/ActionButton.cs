using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class ActionButton : MonoBehaviour
{
    [SerializeField]
    private TMP_Text displayText;
    [SerializeField]
    private Image image;

    private Button _button;
    public ActionData Action => _action;
    private ActionData _action;
    private UnityAction<ActionData> _callback;

    [SerializeField] private Sprite defaultImage;
    [SerializeField] private Sprite selectedImage;

    private void Awake()
    {
        if (_button == null)
        {
            _button = GetComponent<Button>();
        }
    }

    public void SetInactive()
    {
        image.sprite = defaultImage;
    }

    /// <summary>
    /// For an interactable button
    /// </summary>
    /// <param name="action"></param>
    /// <param name="callback"></param>
    public void SetDataButton(ActionData action, UnityAction<ActionData> callback)
    {
        SetDataDisplay(action);

        if (_button == null)
        {
            _button = GetComponent<Button>();
        }
        _button.interactable = true;

        _callback = callback;
        _button.onClick.AddListener(HandleClick);
    }

    /// <summary>
    /// For Display
    /// </summary>
    public void SetDataDisplay(ActionData action)
    {
        _action = action;
        displayText.text = action.actionName;
    }

    private void HandleClick()
    {
        image.sprite = selectedImage;
        _callback(_action);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveAllListeners();
    }
}
