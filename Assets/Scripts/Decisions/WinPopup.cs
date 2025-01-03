using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinPopup : MonoBehaviour
{
    [SerializeField] private Button _acceptVictoryButton;
    [SerializeField] private Button _continuePlayingButton;

    private void Start()
    {
        _acceptVictoryButton.onClick.AddListener(ReturnToMenu);
        _continuePlayingButton.onClick.AddListener(ClosePopup);
    }

    private void OnDestroy()
    {
        _acceptVictoryButton.onClick.RemoveListener(ReturnToMenu);
        _continuePlayingButton.onClick.RemoveListener(ClosePopup);
    }

    private void ReturnToMenu()
    {
        GameManager.Instance.LoadMainMenu();
    }

    private void ClosePopup()
    {
        gameObject.SetActive(false);
    }
}
