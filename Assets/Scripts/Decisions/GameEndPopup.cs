using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameEndPopup : MonoBehaviour
{
    [SerializeField] private Button _acceptGameEndButton;
    [SerializeField] private Button _continuePlayingButton;
    [SerializeField] private TMP_Text _text;
    [SerializeField] private TMP_Text _title;

    private void Start()
    {
        _acceptGameEndButton.onClick.AddListener(ReturnToMenu);
        _continuePlayingButton.onClick.AddListener(ClosePopup);
    }

    private void OnDestroy()
    {
        _acceptGameEndButton.onClick.RemoveListener(ReturnToMenu);
        _continuePlayingButton.onClick.RemoveListener(ClosePopup);
    }

    public void SetPlayerWon(bool playerWon)
    {
        if (playerWon)
        {
            _continuePlayingButton.gameObject.SetActive(true);
            _title.text = "You Win!";
            _text.text = "You now have enough gold for you to retire comfortably and live out your days in peace. Will you put your ambition aside, finally able to enjoy peace, or will you pick up the sword once more and continue to fight in the name of gold and glory?";
        }
        else
        {
            _continuePlayingButton.gameObject.SetActive(false);            
            _title.text = "You Lose!";
            _text.text = "You do not have the funds to recruit, and you have lost all your warriors. Defeat!";
        }
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
