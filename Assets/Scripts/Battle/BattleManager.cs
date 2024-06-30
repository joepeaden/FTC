using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    bool isPlayerTurn = false;

    public static BattleManager Instance => _instance;
    private static BattleManager _instance;
    private Pawn _activePawn;

    [SerializeField] TMP_Text turnText;
    [SerializeField] TMP_Text winLoseText;
    [SerializeField] GameObject winLoseUI;
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private List<Pawn> friendlyPawns;

    private void Awake()
    {
        _instance = this;
    }

    private void OnEnable()
    {
        StartCoroutine(BeginBattle());
    }

    public void PawnActivated(Pawn newPawn)
    {
        _activePawn = newPawn;

        //if (newPawn.ActionsThisTurn >= 2)
        //{

        if (SelectionManager.SelectedTile != null)
        {
            SelectionManager.SelectedTile.SetSelected(false);
        }

        StartCoroutine(SwapTurns());
        //}
    }

    private IEnumerator SwapTurns()
    {

        SelectionManager.Instance.DisablePlayerControls();
        yield return new WaitForSeconds(1f);

        if (enemyAI.GetLivingPawns().Count <= 0)
        {
            turnText.gameObject.SetActive(false);
            winLoseUI.SetActive(true);
            winLoseText.text = "Victory!";
        }
        else if (true)
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
                turnText.gameObject.SetActive(false);
                winLoseUI.SetActive(true);
                winLoseText.text = "Defeat!";
            }
        }

        if (!isPlayerTurn)
        {
            StartPlayerTurn();
        }
        else
        {
            StartEnemyTurn();
        }
    }

    private IEnumerator BeginBattle()
    {
        yield return new WaitUntil(() => SelectionManager.Instance != null);
        StartPlayerTurn();
    }

    void StartPlayerTurn()
    {
        isPlayerTurn = true;
        SelectionManager.Instance.HandleTurnChange(true);
        turnText.text = "Player Turn";
    }

    void StartEnemyTurn()
    {
        isPlayerTurn = false;
        SelectionManager.Instance.HandleTurnChange(false);
        enemyAI.DoTurn();
        turnText.text = "Enemy Turn";
    }
}
