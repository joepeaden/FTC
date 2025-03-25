using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class DecisionPanel : MonoBehaviour
{
    enum DecisionType
    {
        Contract,
        Recruit
    }
    private DecisionType _decisionType;

    public UnityEvent<GameCharacter> OnRecruit = new();

    [SerializeField] private int _maxNumOfEnemies;
    [SerializeField] private int _minNumOfEnemies;
    [SerializeField] private TMP_Text _numOfEnemiesTxt;
    [SerializeField] private TMP_Text _goldAmountText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private PawnPreview _pawnPreview;

    int numOfEnemies;

    private List<GameCharacter> _enemies;

    int goldAmount;

    private GameCharacter _recruit;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(HandleClick);
    }

    public void GenerateRecruitOption()
    {
        _recruit = new(DataLoader.charTypes["player"]);

        if (GameManager.Instance != null)
        {
            goldAmount = GameManager.Instance.GameCharData.recruitPrice;
        }
        else
        {
            goldAmount = 100;
        }

        _numOfEnemiesTxt.gameObject.SetActive(false);

        _titleText.text = _recruit.CharName;

        //switch (_recruit.Motivator)
        //{
        //    case GameCharacter.CharMotivators.Greed:
        //        _descriptionText.text = _recruit.CharName + " desires wealth and posessions above all other things.";
        //        break;
        //    case GameCharacter.CharMotivators.Honor:
        //        _descriptionText.text = _recruit.CharName + " is righteous and honorable - and full of arrogance.";
        //        break;
        //    case GameCharacter.CharMotivators.Glory:
        //        _descriptionText.text = _recruit.CharName + " wishes the crowds to know his name, be it by amazing deed or brutal death.";
        //        break;
        //}

        _goldAmountText.text = "Cost: " + goldAmount + " gold";

        _decisionType = DecisionType.Recruit;

        _pawnPreview.SetData(_recruit);
    }

    public void GenerateContractOption()
    {
        numOfEnemies = Random.Range(_minNumOfEnemies, _maxNumOfEnemies);

        _enemies = GenerateEnemies(numOfEnemies);

        goldAmount = numOfEnemies * 25;

        _titleText.text = "Contract";
        _descriptionText.text = "Here here! Local ruffians have disturbed m'lord's lands and terrorized the people of this here village. A group of men is hereby requested to deal with them.";
        _numOfEnemiesTxt.gameObject.SetActive(true);
        _numOfEnemiesTxt.text = "x " + numOfEnemies;
        _goldAmountText.text = "Reward: " + goldAmount + " gold";

        _decisionType = DecisionType.Contract;

        _pawnPreview.SetData(new GameCharacter(DataLoader.charTypes["thrall"]));
    }

    private List<GameCharacter> GenerateEnemies(int numToGenerate)
    {
        List<GameCharacter> enemies = new();
        for (int i = 0; i < numToGenerate; i++)
        {
            GameCharacter guy = new(DataLoader.charTypes["thrall"]);

            // pick random armor
            //roll = Random.Range(0, 4);
            //switch (roll)
            //{
            //    case 0:
            //        // no armor
            //        break;
            //    case 1:
            //        guy.EquipItem(GameManager.Instance.EquipmentList.lightHelm);
            //        break;
            //    case 2:
            //        guy.EquipItem(GameManager.Instance.EquipmentList.heavyHelm);
            //        break;
            //    case 3:
            //        guy.EquipItem(GameManager.Instance.EquipmentList.medHelm);
            //        break;
            //}

            enemies.Add(guy);
        }

        return enemies;
    }

    private void HandleClick()
    {
        switch (_decisionType)
        {
            case DecisionType.Contract:
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.LoadBattle(_enemies, goldAmount);
                }
                else
                {
                    // in case not starting from bootstrap (for testing)
                    SceneManager.LoadScene("Battle");
                    Debug.Log("Not started from bootstrap!");
                }
                break;
            case DecisionType.Recruit:
                if (GameManager.Instance != null)
                {
                    GameCharacter newFollower = GameManager.Instance.TryAddFollower(goldAmount, _recruit);

                    if (newFollower != null)
                    {
                        OnRecruit.Invoke(newFollower);
                        gameObject.SetActive(false);
                    }
                }

                break;
        }
    }

    private void OnDestroy()
    {
        GetComponent<Button>().onClick.RemoveListener(HandleClick);
        OnRecruit.RemoveAllListeners();
    }

}
