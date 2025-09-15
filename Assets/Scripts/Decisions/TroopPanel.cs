using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public class TroopPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text _troopNameText;
    [SerializeField] private Button _theButton;
    [SerializeField] private PawnPreview _preview;
    [SerializeField] private GameObject _levelUpObjects;

    private GameCharacter _gameChar;

    public void SetupPanel(GameCharacter gameChar, UnityAction<GameCharacter> callback)
    {
        _troopNameText.text = gameChar.CharName;
        _theButton.onClick.AddListener(() => { callback(gameChar); });

        _preview.SetData(gameChar);

        _levelUpObjects.SetActive(gameChar.PendingPerkChoices > 0 || gameChar.PendingStatChoices > 0);

        _gameChar = gameChar;
    }

    private void OnEnable()
    {
        // when going back and forth between the equip screen and the troops screen, need to update the character gear.
        if (_gameChar != null)
        {
            _preview.SetData(_gameChar);
        }
    }

    private void OnDestroy()
    {
        _theButton.onClick.RemoveAllListeners();
    }
}
