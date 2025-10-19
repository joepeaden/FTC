using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIPlayer : MonoBehaviour
{
    private List<Pawn> _enemyPawns = new();

    public void RegisterPawn(Pawn p)
    {
        _enemyPawns.Add(p);
    }

    public List<Pawn> GetEnemyLivingPawns()
    {
        // OPTIMIZE: could just update the list when a pawn dies, not iterate
        // whole list every time
        List<Pawn> enemyLivingPawns = new();
        foreach (Pawn p in _enemyPawns)
        {
            if (p.IsDead)
            {
                continue;
            }

            enemyLivingPawns.Add(p);
        }

        return enemyLivingPawns;
    }

    public void DoTurn(Pawn p)
    {
        StartCoroutine(DoTurnCoroutine(p));
    }
    
    private IEnumerator DoTurnCoroutine(Pawn activePawn)
    {
        // just add a pause so it's not jarring how fast turns change
        yield return new WaitForSeconds(1f);

        // see if there's anyone adjacent to attack, if so, attack
        Pawn adjacentPawn = GetAdjacentTarget(activePawn);
        if (adjacentPawn != null)
        {
            activePawn.GetWeaponAbilities()[0].Activate(activePawn, adjacentPawn);
        }
        // add comment
        else
        {
            // try to move towards enemy pawns
            List<Pawn> pawnsToMoveTowards = activePawn.OnPlayerTeam ? _enemyPawns : BattleManager.Instance.PlayerPawns;
            List<Tile> potentialTargetTiles = GetTargetTilesTowardsPawns(pawnsToMoveTowards, activePawn);

            // otherwise, we want to move towards an ally pawn (they're probably going somewhere right?)
            // hopefully this doesn't end up making silly things happen. Or, maybe that'd be fun.
            if (potentialTargetTiles.Count == 0)
            {
                pawnsToMoveTowards = activePawn.OnPlayerTeam ? BattleManager.Instance.PlayerPawns : _enemyPawns;
                potentialTargetTiles = GetTargetTilesTowardsPawns(pawnsToMoveTowards, activePawn);
            }

            // if there's no potential tiles to move towrads... just stand there.
            if (potentialTargetTiles.Count > 0)
            {
                Tile selectedTargetTile = potentialTargetTiles.OrderBy(t => activePawn.CurrentTile.GetTileDistance(t)).First();
                
                // need to make sure we select a tile we can actually reach to go there. 
                Tile finalTargetTile = activePawn.GetTileInMoveRangeTowards(selectedTargetTile);

                activePawn.TryMoveToTile(finalTargetTile);
            }
            else
            {
                activePawn.PassTurn();
            }
        }
    }

    private List<Tile> GetTargetTilesTowardsPawns(List<Pawn> pawnsToMoveTowards, Pawn activePawn)
    {
        List<Tile> potentialTargetTiles = new();
        foreach (Pawn targetPawn in pawnsToMoveTowards)
        {
            if (targetPawn.IsDead)
            {
                continue;
            }

            Tile bestTargetTile = null;
            foreach (Tile potentialTargetTile in targetPawn.CurrentTile.GetAdjacentTiles())
            {
                // we don't want to include it if we couldn't get there anyway - this is the case
                // like if we've been surrounded by allies and can't get to any enemies. For whatever
                // reason this target tile is unreachable.
                if (!activePawn.HasPathToTile(potentialTargetTile))
                {
                    continue;
                }

                Pawn pawnAtTile = potentialTargetTile.GetPawn();
                // don't consider if someone's there
                if (pawnAtTile != null && !pawnAtTile.IsDead || !potentialTargetTile.CanTraverse(activePawn))
                {
                    continue;
                }

                // just go with the first one for now
                bestTargetTile = potentialTargetTile;
                break;
            }

            if (bestTargetTile != null)
            {
                potentialTargetTiles.Add(bestTargetTile);
            }
        }

        return potentialTargetTiles;
    }

    /// <summary>
    /// Go through adjacent tiles and return the first player team pawn found,
    /// if none return null
    /// </summary>
    /// <param name="activePawn"></param>
    /// <returns></returns>
    private Pawn GetAdjacentTarget(Pawn activePawn)
    {
        Pawn targetPawn = null;
        foreach (Tile t in activePawn.CurrentTile.GetAdjacentTiles())
        {
            targetPawn = t.GetPawn();
            if (targetPawn != null && targetPawn.OnPlayerTeam != activePawn.OnPlayerTeam)
            {
                break;
            }
            else
            {
                targetPawn = null;
            }
        }
        return targetPawn;
    }


}
