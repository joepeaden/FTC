using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private List<Pawn> _pawns = new();

    public void RegisterPawn(Pawn p)
    {
        _pawns.Add(p);
        p.SetTeam(false);
    }

    public List<Pawn> GetLivingPawns()
    {
        // OPTIMIZE: could just update the list when a pawn dies, not iterate
        // whole list every time
        List<Pawn> pawnsToUse = new();
        foreach (Pawn p in _pawns)
        {
            if (p.IsDead)
            {
                continue;
            }

            pawnsToUse.Add(p);
        }

        return pawnsToUse;
    }

    public void DoTurn()
    {
        List<Pawn> pawnsToUse = GetLivingPawns();

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
            Tile tileToMoveTo;

            for (int i = 0; i < moveOptions.Count; i++)
            {
                Tile t = moveOptions[i];
                if (t.GetPawn() != null)
                {
                    moveOptions.Remove(t);
                }
            }

            // for some reason sometimes the t.GetPawn null check above will
            // fail, so just keep picking random tiles till there's a free one
            do
            {
                tileToMoveTo = moveOptions[Random.Range(0, moveOptions.Count)];
            }
            while (tileToMoveTo.GetPawn() != null) ; 
            

            activePawn.MoveToTile(tileToMoveTo);
        }
    }


}
