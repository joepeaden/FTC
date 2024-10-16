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
    private TMP_Text hotKeyText;
    [SerializeField]
    private Image image;

    private Button _button;
    public ActionData Action => _action;
    private ActionData _action;
    private UnityAction<ActionData> _callback;

    private KeyCode _hotKey;
    private bool _isSelected = false;

    // if it's not button mode then it's only for display purposes, so no interaction etc.
    private bool _isButtonMode = false;

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

    string GetKeyCodeDisplay(KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.Alpha1: return "1";
            case KeyCode.Alpha2: return "2";
            case KeyCode.Alpha3: return "3";
            case KeyCode.Alpha4: return "4";
            case KeyCode.Alpha5: return "5";
            case KeyCode.Alpha6: return "6";
            case KeyCode.Alpha7: return "7";
            case KeyCode.Alpha8: return "8";
            case KeyCode.Alpha9: return "9";
            case KeyCode.Alpha0: return "0";
            default: return keyCode.ToString(); // Default to the keycode name
        }
    }

    private void UpdateInteractivity()
    {
        _button.interactable = BattleManager.Instance.CurrentPawn.HasResourcesForAction(_action);

        // don't look disabled in display mode.
        if (!_isButtonMode)
        {
            _button.GetComponent<Image>().color = Color.white;
        }
    }

    /// <summary>
    /// For an interactable button
    /// </summary>
    /// <param name="action"></param>
    /// <param name="callback"></param>
    public void SetDataButton(ActionData action, UnityAction<ActionData> callback, KeyCode hotkey)
    {
        SetDataDisplay(action);

        if (_button == null)
        {
            _button = GetComponent<Button>();
        }
        _isButtonMode = true;

        UpdateInteractivity();

        if (BattleManager.Instance.CurrentAction == action)
        {
            image.sprite = selectedImage;
        }

        _hotKey = hotkey;
        hotKeyText.text = $"({GetKeyCodeDisplay(hotkey)})";

        _callback = callback;
        _button.onClick.AddListener(HandleClick);
    }

    private void Update()
    {
        // hopefully
        // the weather is nice today
        // I'm craving a breakfast croissant
        if (_isButtonMode && Input.GetKeyDown(_hotKey) && _button.interactable)
        {
            _button.onClick.Invoke();
        }
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
        _isSelected = !_isSelected;

        if (_isSelected)
        {
            _callback(_action);
            image.sprite = selectedImage;
        }
        else
        {
            BattleManager.Instance.ClearSelectedAction();
        }
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveAllListeners();
    }
}
