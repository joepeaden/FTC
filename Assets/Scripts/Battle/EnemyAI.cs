using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyAI : MonoBehaviour
{
    private List<Pawn> _pawns = new();

    public void RegisterPawn(Pawn p)
    {
        _pawns.Add(p);
        GameCharacter guy = new();
        p.SetCharacter(guy, false);
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

    public void DoTurn(Pawn p)
    {
        StartCoroutine(DoTurnCoroutine(p));
    }

    private IEnumerator DoTurnCoroutine(Pawn p)
    {
        // just add a pause so it's not jarring how fast turns change
        yield return new WaitForSeconds(1f);

        Pawn activePawn = p;

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
            activePawn.AttackPawnIfAPAvailable(targetPawn);
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
            

            activePawn.MoveToTileIfAPAvailable(tileToMoveTo);
        }
    }


}
