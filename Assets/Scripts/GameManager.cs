using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance => _instance;
    private static GameManager _instance;

    public static int GOLD_WIN_AMOUNT = 1000;

    // these two buddies could be in a struct or something to keep
    // "current mission" stuff contained.
    private List<GameCharacter> _enemiesForContract;
    private int _potentialRewardAmount;

    public int PlayerGold => _playerGold;
    private int _playerGold;

    public List<GameCharacter> PlayerFollowers => _playerFollowers;
    private List<GameCharacter> _playerFollowers = new();

    public List<ItemData> PlayerInventory => _playerInventory;
    private List<ItemData> _playerInventory = new();

    public GameCharacterData GameCharData => _gameCharData;
    [SerializeField] private GameCharacterData _gameCharData;

    public EquipmentListData EquipmentList => _equipmentListData;
    [SerializeField] private EquipmentListData _equipmentListData;

    private void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadMainMenu();
    }

    public void StartNewGame()//string playerCharName)
    {
#if UNITY_EDITOR
        _playerGold = 200;
#else
        _playerGold = GameCharData.startingGold;
#endif

        // add 3 default characters for player
        for (int i = 0; i < 3; i++)
        {
            TryAddFollower(0, new GameCharacter(true));
        }

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

    public void LoadBattle(List<GameCharacter> enemies, int rewardAmount)
    {
        _enemiesForContract = enemies;
        _potentialRewardAmount = rewardAmount;

        SceneManager.LoadScene("BattleScene");
    }

    public List<GameCharacter> GetEnemiesForContract()
    {
        return _enemiesForContract;
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ExitBattle(bool playerWon)
    {
        SceneManager.LoadScene("DecisionsUI");

        if (playerWon)
        {
            AddGold(_potentialRewardAmount);
        }
    }
}
