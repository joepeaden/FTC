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

    public void SetupPanel(GameCharacter gameChar, UnityAction<GameCharacter> callback)
    {
        _troopNameText.text = gameChar.CharName;
        _theButton.onClick.AddListener(() => { callback(gameChar); });
    }

    private void OnDestroy()
    {
        _theButton.onClick.RemoveAllListeners();
    }
}
