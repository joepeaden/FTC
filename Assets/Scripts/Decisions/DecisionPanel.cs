using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Linq;

public class DecisionPanel : MonoBehaviour
{
    enum DecisionType
    {
        Contract,
        Recruit
    }
    private DecisionType _decisionType;

    public UnityEvent<GameCharacter> OnRecruit = new();

    [SerializeField] private TMP_Text _goldAmountText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private PawnPreview _pawnPreview;

    [SerializeField] private List<EnemyTypePreview> enemyPreviews = new();

    private List<GameCharacter> _enemies = new();

    int goldAmount;

    private GameCharacter _recruit;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(HandleClick);
    }

    public void GenerateRecruitOption()
    {
        _recruit = new(DataLoader.charTypes["player"]);



        //_numOfEnemiesTxt.gameObject.SetActive(false);

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

        _goldAmountText.text = "Cost: " + _recruit.Data.price + " gold";

        _decisionType = DecisionType.Recruit;

        _pawnPreview.gameObject.SetActive(true);
        _pawnPreview.SetData(_recruit);
    }

    public void GenerateContractOption()
    {
        Dictionary<string, ContractData> contracts = DataLoader.contracts;
        ContractData contract = contracts.Values.ToList()[Random.Range(0, DataLoader.contracts.Count)];

        int numOfEnemies = Random.Range(contract.minEnemyCount, contract.maxEnemyCount);

        Dictionary<GameCharacterData, int> enemyTypes = new();

        _enemies.Clear();

        int i;
        for (i = 0; i < numOfEnemies; i++)
        {
            GameCharacter guy = new(contract.possibleEnemyTypes[Random.Range(0, contract.possibleEnemyTypes.Count)]);
            _enemies.Add(guy);
            goldAmount += guy.Data.price;

            if (enemyTypes.Keys.Contains(guy.Data))
            {
                enemyTypes[guy.Data] += 1;
            }
            else
            {
                enemyTypes[guy.Data] = 1;
            }
        }

        _titleText.text = contract.contractTitle;
        _descriptionText.text = contract.description;
        _goldAmountText.text = "Reward: " + goldAmount + " gold";

        _decisionType = DecisionType.Contract;

        i = 0;
        foreach (GameCharacterData enemyType in enemyTypes.Keys)
        {
            enemyPreviews[i].gameObject.SetActive(true);
            enemyPreviews[i].SetData(enemyType, enemyTypes[enemyType]);
            i++;
        }

        for (; i < enemyPreviews.Count; i++)
        {
            enemyPreviews[i].gameObject.SetActive(false);
        }
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
