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
    public Ability TheAbility => _ability;
    private Ability _ability;
    private UnityAction<Ability> _callback;

    private KeyCode _hotkey;
    public bool IsSelected => _isSelected;
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

        _button.onClick.AddListener(HandleClick);
    }

    public void SetInactive()
    {
        //image.sprite = defaultImage;
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
        _button.interactable = BattleManager.Instance.CurrentPawn.HasResourcesForAttackAction(_ability);

        // don't look disabled in display mode.
        if (!_isButtonMode)
        {
            _button.GetComponent<Image>().color = Color.white;
        }
    }

    public void SetSelected (bool isSelected)
    {
        _isSelected = false;
    }

    /// <summary>
    /// For an interactable button
    /// </summary>
    /// <param name="action"></param>
    /// <param name="callback"></param>
    public void SetDataButton(Ability action, UnityAction<Ability> callback, int hotkeyNum)
    {        
        SetDataDisplay(action);

        if (_button == null)
        {
            _button = GetComponent<Button>();
        }
        _isButtonMode = true;

        UpdateInteractivity();

        if (Ability.SelectedAbility == action)
        {
            //image.sprite = selectedImage;
        }

        switch (hotkeyNum)
        {
            case 1:
                _hotkey = KeyCode.Alpha1;
                break;
            case 2:
                _hotkey = KeyCode.Alpha2;
                break;
            case 3:
                _hotkey = KeyCode.Alpha3;
                break;
            case 4:
                _hotkey = KeyCode.Alpha4;
                break;
        }

        hotKeyText.text = $"{GetKeyCodeDisplay(_hotkey)}";

        _callback = callback;
    }

    private void Update()
    {
        // hopefully
        // the weather is nice today
        // I'm craving a breakfast croissant
        if (_isButtonMode && Input.GetKeyDown(_hotkey) && _button.interactable)
        {
            _button.onClick.Invoke();
        }
    }

    /// <summary>
    /// For Display
    /// </summary>
    public void SetDataDisplay(Ability action)
    {
        _ability = action;

        image.sprite = _ability.GetData().sprite;

        //displayText.text = action.GetData().abilityName;
    }

    private void HandleClick()
    {
        _isSelected = !_isSelected;

        if (_isSelected)
        {
            _callback(_ability);
            //image.sprite = selectedImage;
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
