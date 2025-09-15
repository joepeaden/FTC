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

    [SerializeField] private AudioClip _clickSound;

    private AudioSource _musicPlayer;
    [SerializeField] private AudioClip _menuMusic;
    [SerializeField] private AudioClip _battleMusic;

    public int CurrentDay => _currentDay;
    private int _currentDay = 0;

    // perhaps this value should be in a scriptable. Idk if there's one set up yet for these kinds of 
    // values.
    // The cost per character level that the player must pay each day
    private int _levelCostMultiplier = 10;

    private void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);
        _musicPlayer = GetComponent<AudioSource>();
    }

    private void Start()
    {
        DataLoader dataLoader = new DataLoader();
        dataLoader.LoadData();

        LoadMainMenu();
    }

    /// <summary>
    /// Refresh gold, day, and followers and start a new game.
    /// </summary>
    public void StartNewGame()
    {
        _playerGold = GameCharData.startingGold;
        _currentDay = 0;

        // add 3 default characters for player
        for (int i = 0; i < 3; i++)
        {
            TryAddFollower(0, new GameCharacter(DataLoader.charTypes["player"]));
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

    public void NextDay()
    {
        // make the player pay for their total follower levels.
        _playerGold -= GetPayout();
        _currentDay++;
    }

    public int GetWageForCharacter(GameCharacter character)
    {
        // character.Level+1 because initially, they're level 0
        return _levelCostMultiplier * (character.Level + 1);
    }

    /// <summary>
    /// Get the money that the player owes during the next day.
    /// </summary>
    /// <returns></returns>
    public int GetPayout()
    {
        int totalPayout = 0;

        foreach (GameCharacter character in PlayerFollowers)
        {
            // character.Level+1 because initially, they're level 0
            totalPayout += (character.Level+1) * _levelCostMultiplier;
        }

        return totalPayout;
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

        _musicPlayer.clip = _battleMusic;
        _musicPlayer.Play();
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

        // pass a day after a mission (which increments the day and triggers a payout)
        NextDay();

        _musicPlayer.clip = _menuMusic;
        _musicPlayer.Play();
    }

    public void PlayClickEffect()
    {
        GameObject pooledAudioSourceGO = ObjectPool.instance.GetAudioSource();
        pooledAudioSourceGO.SetActive(true);
        AudioSource audioSource = pooledAudioSourceGO.GetComponent<AudioSource>();
        audioSource.clip = _clickSound;
        audioSource.Play();
    }
}
