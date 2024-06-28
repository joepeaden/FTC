using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    bool isPlayerTurn = false;

    public static BattleManager Instance => _instance;
    private static BattleManager _instance;

    private Pawn _activePawn;

    private void Awake()
    {
        _instance = this;
    }

    private void OnEnable()
    {
        StartCoroutine(BeginBattle());
    }

    public void PawnMoved(Pawn newPawn)
    {
        _activePawn = newPawn;

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
    }

    void StartEnemyTurn()
    {
        isPlayerTurn = false;
        SelectionManager.Instance.HandleTurnChange(false);
        EnemyAI.Instance.DoTurn();
    }
}
