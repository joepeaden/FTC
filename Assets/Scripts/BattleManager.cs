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
        EnemyAI.Instance.DoTurn();
        turnText.text = "Enemy Turn";
    }
}
