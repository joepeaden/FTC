using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelUpCard : MonoBehaviour
{
    [SerializeField]
    private TMP_Text title;
    [SerializeField]
    private TMP_Text description;
    [SerializeField]
    private Image image;

    private LevelUpPanel _lvlUpPanel;
    private Ability _ability;
    private Button _button;

    public void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(HandleClick);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(HandleClick);
    }

    public void SetData(Ability ability, LevelUpPanel lvlUpPanel)
    {
        title.text = ability.abilityName;
        image.sprite = ability.sprite;
        description.text = ability.description;

        _ability = ability;
        _lvlUpPanel = lvlUpPanel;
    }

    private void HandleClick()
    {
        _lvlUpPanel.HandleLvlUpCardSelected(_ability);
    }
}
