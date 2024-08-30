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
    private int _playerGold;

    public GameCharacter PlayerCharacter => _playerCharacter;
    private GameCharacter _playerCharacter;

    public List<GameCharacter> PlayerFollowers => _playerFollowers;
    private List<GameCharacter> _playerFollowers = new();

    public List<ItemData> PlayerInventory => _playerInventory;
    private List<ItemData> _playerInventory = new();

    public GameCharacterData GameCharData => _gameCharData;
    [SerializeField] private GameCharacterData _gameCharData;

    private void Awake()
    {
        _instance = this;

        DontDestroyOnLoad(gameObject);

        SceneManager.LoadScene("MainMenu");
    }

    public void StartNewGame(string playerCharName)
    {
        _playerCharacter = new (playerCharName, true, Random.Range(_gameCharData.minVice, _gameCharData.maxVice), Random.Range(_gameCharData.minVice, _gameCharData.maxVice), Random.Range(_gameCharData.minVice, _gameCharData.maxVice));
        _playerGold = GameCharData.startingGold;
        SceneManager.LoadScene("DecisionsUI");
    }

    public GameCharacter TryAddFollower(int cost, GameCharacter newFollower)
    {
        int goldRemaining = _playerGold - cost;
        if (goldRemaining >= 0)
        {
            _playerFollowers.Add(newFollower);
            _playerGold = goldRemaining;

            return newFollower;
        }

        return null;
    }

    public void AddGold(int amount)
    {
        _playerGold += amount;
    }

    public void RemoveItem(ItemData item)
    {
        if (PlayerInventory.Contains(item))
        {
            PlayerInventory.Remove(item);
        }
    }

    public bool TryAddItem(ItemData item)
    {
        if (item.itemPrice > _playerGold)
        {
            return false;
        }

        _playerGold -= item.itemPrice;

        // add item to player inventory data structure
        PlayerInventory.Add(item);

        return true;
    }

    public void RemoveFollower(GameCharacter follower)
    {
        if (_playerFollowers.Contains(follower))
        {
            _playerFollowers.Remove(follower);
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
