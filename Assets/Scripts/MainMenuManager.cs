using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button _newGameButton;
    [SerializeField] private Button _startGameFromCharCreateButton;
    [SerializeField] private TMP_InputField _newCharNameInput;

    [SerializeField] private GameObject _mainMenuScreen;
    [SerializeField] private GameObject _characterCreationScreen;

    private void Awake()
    {
        _newGameButton.onClick.AddListener(StartGame);//ShowCharacterCreation);
        //_startGameFromCharCreateButton.onClick.AddListener(StartGameFromCharCreation);
    }

    //private void ShowCharacterCreation()
    //{
    //    _mainMenuScreen.SetActive(false);
    //    _characterCreationScreen.SetActive(true);
    //}

    private void StartGame()
    {
        //if (_newCharNameInput.text == string.Empty)
        //{
        //    return;
        //}

        GameManager.Instance.StartNewGame();// _newCharNameInput.text);
    }

    private void OnDestroy()
    {
        _newGameButton.onClick.RemoveListener(StartGame);//ShowCharacterCreation);
        //_startGameFromCharCreateButton.onClick.RemoveListener(StartGame);
    }
}
