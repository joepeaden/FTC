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

    public UnityEvent OnRecruit = new();

    [SerializeField] private int _maxNumOfEnemies;
    [SerializeField] private int _minNumOfEnemies;
    [SerializeField] private TMP_Text _numOfEnemiesTxt;
    [SerializeField] private TMP_Text _goldAmountText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _titleText;

    int numOfEnemies;
    int goldAmount;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(HandleClick);
    }

    public void GenerateRecruitOption()
    {
        goldAmount = 100;

        _numOfEnemiesTxt.gameObject.SetActive(false);

        _titleText.text = "Recruit";
        _descriptionText.text = "A local man looking for some good pay and a good fight to go with it.";
        _goldAmountText.text = "Cost: " + goldAmount + " gold";

        _decisionType = DecisionType.Recruit;
    }

    public void GenerateContractOption()
    {
        numOfEnemies = Random.Range(_minNumOfEnemies, _maxNumOfEnemies);
        goldAmount = numOfEnemies * 100;

        _titleText.text = "Contract";
        _descriptionText.text = "Here here! Local ruffians have disturbed m'lord's lands and terrorized the people of this here village. A group of men is hereby requested to deal with them.";
        _numOfEnemiesTxt.gameObject.SetActive(true);
        _numOfEnemiesTxt.text = "x " + numOfEnemies;
        _goldAmountText.text = "Reward: " + goldAmount + " gold";

        _decisionType = DecisionType.Contract;
    }

    private void HandleClick()
    {

        switch (_decisionType)
        {
            case DecisionType.Contract:
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.LoadBattle(numOfEnemies, goldAmount);
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
                    GameManager.Instance.TryAddFollower(goldAmount);
                }

                OnRecruit.Invoke();

                break;
        }
    }

    private void OnDestroy()
    {
        GetComponent<Button>().onClick.RemoveListener(HandleClick);
        OnRecruit.RemoveAllListeners();
    }

}
