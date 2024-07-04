using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DecisionPanel : MonoBehaviour
{
    [SerializeField] int maxNumOfEnemies;
    [SerializeField] int minNumOfEnemies;
    [SerializeField] TMP_Text numOfEnemiesTxt;
    [SerializeField] TMP_Text rewardAmountTxt;

    int numOfEnemies;
    int rewardAmount;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(LoadBattle);
    }

    public void GenerateOption()
    {
        numOfEnemies = Random.Range(minNumOfEnemies, maxNumOfEnemies);
        rewardAmount = numOfEnemies * 100;

        numOfEnemiesTxt.text = "x " + numOfEnemies;
        rewardAmountTxt.text = "Reward: " + rewardAmount + " gold";
    }

    private void LoadBattle()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadBattle(numOfEnemies);
        }
        else
        {
            // in case not starting from bootstrap (for testing)
            SceneManager.LoadScene("Battle");
            Debug.Log("Not started from bootstrap!");
        }
    }

    private void OnDestroy()
    {
        GetComponent<Button>().onClick.RemoveListener(LoadBattle);
    }

}
