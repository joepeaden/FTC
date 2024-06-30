using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DecisionPanel : MonoBehaviour
{
    [SerializeField] int maxNumOfEnemies;
    [SerializeField] int minNumOfEnemies;
    [SerializeField] TMP_Text numOfEnemiesTxt;
    [SerializeField] TMP_Text rewardAmountTxt;

    int numOfEnemies;
    int rewardAmount;

    public void GenerateOption()
    {
        numOfEnemies = Random.Range(minNumOfEnemies, maxNumOfEnemies);
        rewardAmount = numOfEnemies * 100;

        numOfEnemiesTxt.text = "x " + numOfEnemies;
        numOfEnemiesTxt.text = rewardAmount + " gold";
    }

    private void OnMouseDown()
    {
        SceneManager.LoadScene("BattleScene");
    }

}
