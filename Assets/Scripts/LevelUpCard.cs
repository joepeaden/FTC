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
    public Ability ability;
    public PassiveData passive;
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

    public void SetData(PassiveData passive, LevelUpPanel lvlUpPanel)
    {
        this.ability = null;
        title.text = passive.displayName;
        image.gameObject.SetActive(false);
        // image.sprite = passive.sprite;
        description.text = passive.description;

        this.passive = passive;
        _lvlUpPanel = lvlUpPanel;
    }

    public void SetData(Ability ability, LevelUpPanel lvlUpPanel)
    {
        this.passive = null;
        title.text = ability.abilityName;
        image.sprite = ability.sprite;
        description.text = ability.description;

        this.ability = ability;
        _lvlUpPanel = lvlUpPanel;
    }

    private void HandleClick()
    {
        _lvlUpPanel.HandleLvlUpCardSelected(this);
    }
}
