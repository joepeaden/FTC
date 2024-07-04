using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
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
        }
        else
        {
            Debug.Log("No game manager, spawning default amount");
            enemiesToSpawn = 4;
        }

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Pawn newPawn = Instantiate(pawnPrefab, enemyParent).GetComponent<Pawn>();
            enemyAI.RegisterPawn(newPawn);
        }

        for (int i = 0; i < 4; i++)
        {
            Pawn newPawn = Instantiate(pawnPrefab, friendlyParent).GetComponent<Pawn>();
            friendlyPawns.Add(newPawn);
            newPawn.SetTeam(true);
        }
    }

    private void OnEnable()
    {
        StartCoroutine(BeginBattle());
    }

    private void OnDestroy()
    {
        gameOverButton.onClick.RemoveListener(ExitBattle);
    }

    private void ExitBattle()
    {
        SceneManager.LoadScene("DecisionsUI");
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

    private IEnumerator BeginBattle()
    {
        yield return new WaitUntil(() => _selectionManager != null);
        StartPlayerTurn();
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
