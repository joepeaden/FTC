using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance => _instance;
    private static GameManager _instance;

    // these two buddies could be in a struct or something to keep
    // "current mission" stuff contained.
    private int _enemiesToSpawn;
    private int _potentialRewardAmount;

    public int PlayerGold => _playerGold;
    [SerializeField] private int _playerGold = 100;

    public CharInfo PlayerCharacter => _playerCharacter;
    private CharInfo _playerCharacter;

    public List<CharInfo> PlayerFollowers => _playerFollowers;
    private List<CharInfo> _playerFollowers = new();


    private void Awake()
    {
        _instance = this;

        DontDestroyOnLoad(gameObject);

        SceneManager.LoadScene("MainMenu");
    }

    public void StartNewGame(string playerCharName)
    {
        _playerCharacter = new (playerCharName, true);
        SceneManager.LoadScene("DecisionsUI");
    }

    public void TryAddFollower(int cost)
    {
        int goldRemaining = _playerGold - cost;
        if (goldRemaining >= 0)
        {
            CharInfo newFollower = new("Bob", false);
            _playerFollowers.Add(newFollower);
            _playerGold = goldRemaining;
        }
    }

    public void LoadBattle(int numOfEnemies, int rewardAmount)
    {
        _enemiesToSpawn = numOfEnemies;
        _potentialRewardAmount = rewardAmount;

        SceneManager.LoadScene("BattleScene");
    }

    public int GetNumOfEnemiesToSpawn()
    {
        return _enemiesToSpawn;
    }

    public void ExitBattle(bool playerWon)
    {
        if (playerWon)
        {
            _playerGold += _potentialRewardAmount;
        }

        SceneManager.LoadScene("DecisionsUI");
    }
}
