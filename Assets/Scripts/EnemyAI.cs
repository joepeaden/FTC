using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public static EnemyAI Instance => _instance;
    private static EnemyAI _instance;

    [SerializeField] private List<Pawn> pawns = new();

    private void Awake()
    {
        _instance = this;
    }

    public void DoTurn()
    {
        Pawn activePawn = pawns[Random.Range(0, pawns.Count)];
        List<Tile> moveOptions = activePawn.CurrentTile.GetTilesInMoveRange();

        Tile tileToMoveTo = moveOptions[Random.Range(0, moveOptions.Count)];
        activePawn.ActOnTile(tileToMoveTo);
    }


}
