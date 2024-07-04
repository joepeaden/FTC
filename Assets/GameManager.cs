using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance => _instance;
    private static GameManager _instance;

    private int _enemiesToSpawn;

    private void Awake()
    {
        _instance = this;

        DontDestroyOnLoad(gameObject);

        SceneManager.LoadScene("DecisionsUI");
    }

    public void LoadBattle(int numOfEnemies)
    {
        _enemiesToSpawn = numOfEnemies;

        SceneManager.LoadScene("BattleScene");
    }

    public int GetNumOfEnemiesToSpawn()
    {
        return _enemiesToSpawn;
    }
}
