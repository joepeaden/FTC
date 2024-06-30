using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public static EnemyAI Instance => _instance;
    private static EnemyAI _instance;

    [SerializeField] private List<Pawn> _pawns = new();

    private void Awake()
    {
        _instance = this;
    }

    public void DoTurn()
    {
        List<Pawn> pawnsToUse = new();
        foreach (Pawn p in _pawns)
        {
            if (p.IsDead)
            {
                continue;
            }

            pawnsToUse.Add(p);
        }


        Pawn activePawn = pawnsToUse[Random.Range(0, pawnsToUse.Count)];

        // see if there's anyone adjacent to attack
        Pawn targetPawn = null;
        bool hasFoundTarget = false;
        foreach (Tile t in activePawn.CurrentTile.GetAdjacentTiles())
        {
            targetPawn = t.GetPawn();
            if (targetPawn != null && targetPawn.OnPlayerTeam)
            {
                hasFoundTarget = true;
                break;
            }
        }

        if (hasFoundTarget)
        {
            activePawn.AttackPawn(targetPawn);
        }
        else
        {
            // At this point, we have not attacked anyone, so find somewhere to move
            List<Tile> moveOptions = activePawn.CurrentTile.GetTilesInMoveRange();
            for (int i = 0; i < moveOptions.Count; i++)
            {
                Tile t = moveOptions[i];
                if (t.GetPawn() != null)
                {
                    moveOptions.Remove(t);
                }
            }

            Tile tileToMoveTo = moveOptions[Random.Range(0, moveOptions.Count)];
            activePawn.MoveToTile(tileToMoveTo);
        }
    }


}
