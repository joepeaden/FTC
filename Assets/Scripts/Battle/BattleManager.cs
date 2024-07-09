using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    private const int DEFAULT_AMOUNT_TO_SPAWN = 4;

    enum BattleResult
    {
        Win,
        Lose,
        Undecided
    };
    private BattleResult _battleResult;

    bool isPlayerTurn = false;

    public static BattleManager Instance => _instance;
    private static BattleManager _instance;
    private List<Pawn> friendlyPawns = new();

    [SerializeField] TMP_Text turnText;
    [SerializeField] GameObject turnUI;
    [SerializeField] TMP_Text winLoseText;
    [SerializeField] GameObject winLoseUI;
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private Button gameOverButton;
    [SerializeField] private SelectionManager _selectionManager;
    [SerializeField] GameObject pawnPrefab;
    [SerializeField] Transform enemyParent;
    [SerializeField] Transform friendlyParent;

    private void Awake()
    {
        _instance = this;
        gameOverButton.onClick.AddListener(ExitBattle);

        int enemiesToSpawn;
        if (GameManager.Instance != null)
        {
            enemiesToSpawn = GameManager.Instance.GetNumOfEnemiesToSpawn();

            Pawn playerPawn = Instantiate(pawnPrefab, friendlyParent).GetComponent<Pawn>();
            friendlyPawns.Add(playerPawn);
            playerPawn.SetCharacter(GameManager.Instance.PlayerCharacter);

            foreach (CharInfo character in GameManager.Instance.PlayerFollowers)
            {
                Pawn newPawn = Instantiate(pawnPrefab, friendlyParent).GetComponent<Pawn>();
                friendlyPawns.Add(newPawn);
                newPawn.SetCharacter(character);
            }
        }
        else
        {
            Debug.Log("No game manager, spawning default amount");
            enemiesToSpawn = DEFAULT_AMOUNT_TO_SPAWN;

            // spawn some friendlies
            for (int i = 0; i < DEFAULT_AMOUNT_TO_SPAWN; i++)
            {
                Pawn newPawn = Instantiate(pawnPrefab, friendlyParent).GetComponent<Pawn>();
                friendlyPawns.Add(newPawn);
                newPawn.SetTeam(true);
            }
        }

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Pawn newPawn = Instantiate(pawnPrefab, enemyParent).GetComponent<Pawn>();
            enemyAI.RegisterPawn(newPawn);
        }
    }

    private void OnEnable()
    {
        _battleResult = BattleResult.Undecided;
        StartPlayerTurn();
    }

    private void OnDestroy()
    {
        gameOverButton.onClick.RemoveListener(ExitBattle);
    }

    private void ExitBattle()
    {
        bool playerWon = _battleResult == BattleResult.Win;

        GameManager.Instance.ExitBattle(playerWon);
    }

    public void PawnActivated()
    {
        if (SelectionManager.SelectedTile != null)
        {
            SelectionManager.SelectedTile.SetSelected(false);
        }

        StartCoroutine(SwapTurns());
    }

    private IEnumerator SwapTurns()
    {

        _selectionManager.DisablePlayerControls();
        yield return new WaitForSeconds(1f);

        // see if the battle is over. If so, do sumthin about it 
        if (enemyAI.GetLivingPawns().Count <= 0)
        {
            turnUI.SetActive(false);
            winLoseUI.SetActive(true);
            winLoseText.text = "Victory!";
            _battleResult = BattleResult.Win;
        }
        else
        {
            int alivePawns = 0;
            foreach (Pawn p in friendlyPawns)
            {
                if (p.IsDead)
                {
                    continue;
                }
                alivePawns++;
            }

            if (alivePawns <= 0)
            {
                turnUI.SetActive(false);
                winLoseUI.SetActive(true);
                winLoseText.text = "Defeat!";
                _battleResult = BattleResult.Lose;
            }
            else
            {
                if (!isPlayerTurn)
                {
                    StartPlayerTurn();
                }
                else
                {
                    StartEnemyTurn();
                }
            }
        }
    }

    void StartPlayerTurn()
    {
        isPlayerTurn = true;
        _selectionManager.HandleTurnChange(true);
        turnText.text = "Player Turn";
    }

    void StartEnemyTurn()
    {
        isPlayerTurn = false;
        _selectionManager.HandleTurnChange(false);
        enemyAI.DoTurn();
        turnText.text = "Enemy Turn";
    }
}
