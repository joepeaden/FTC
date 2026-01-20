using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

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

        // for now, enemies should always move towards the castle. Player pawns should obviously move towards enemies
        bool moveTowardsPawn = activePawn.OnPlayerTeam;

        if (IsAdjacentToCastle(activePawn) && !activePawn.OnPlayerTeam)
        {
            activePawn.AttackCastle();
        }
        else
        {
            // see if there's anyone adjacent to attack, if so, attack. Otherwise, go towards a target
            Pawn adjacentPawn = GetAdjacentTarget(activePawn);
            if (adjacentPawn != null)
            {
                activePawn.GetWeaponAbilities()[0].Activate(activePawn, adjacentPawn);
            }
            else
            {
                Move(activePawn, moveTowardsPawn);
            }
        }
    }

    private bool IsAdjacentToCastle(Pawn activePawn)
    {
        return GridGenerator.Instance.CastleTiles.Contains(activePawn.CurrentTile);
    }

    /// <summary>
    /// Move the activePawn.
    /// </summary>
    /// <param name="activePawn">The pawn that's moving</param>
    /// <param name="shouldMoveTowardsPawn">Move towards a pawn? If not, move towards castle</param>
    private void Move(Pawn activePawn, bool shouldMoveTowardsPawn)
    {
        List<Tile> potentialTargetTiles = GetPotentialTilesForMove(activePawn, shouldMoveTowardsPawn);

        if (potentialTargetTiles.Count > 0)
        {
            Tile closestTargetTile = potentialTargetTiles.OrderBy(t => activePawn.CurrentTile.GetTileDistance(t)).First();

            Tile finalTargetTile = activePawn.GetTileInMoveRangeClosestTo(closestTargetTile);
            bool moveStarted = activePawn.TryMoveToTile(finalTargetTile);

            // for some reason we didn't start move - fallback case.
            if (!moveStarted)
            {
                activePawn.PassTurn();
            }
        }
        else
        {
            // nowhere to go!
            activePawn.PassTurn();
        }
    }

    private List<Tile> GetPotentialTilesForMove(Pawn activePawn, bool shouldMoveTowardsPawn)
    {
        List<Pawn> pawnsToMoveTowards;
        List<Tile> potentialTargetTiles;
        if (shouldMoveTowardsPawn)
        {
            // try to move towards enemy pawns
            pawnsToMoveTowards = activePawn.OnPlayerTeam ? _enemyPawns : BattleManager.Instance.PlayerPawns;
            potentialTargetTiles = GetTargetTilesTowardsPawns(pawnsToMoveTowards, activePawn);
        }
        else
        {
            potentialTargetTiles = GetAvailableCastleTiles(activePawn);
        }

        // otherwise, we want to move towards an ally pawn (they're probably going somewhere right?)
        // hopefully this doesn't end up making silly things happen. 
        if (potentialTargetTiles.Count == 0)
        {
            pawnsToMoveTowards = activePawn.OnPlayerTeam ? BattleManager.Instance.PlayerPawns : _enemyPawns;
            potentialTargetTiles = GetTargetTilesTowardsPawns(pawnsToMoveTowards, activePawn);
        }
        
        return potentialTargetTiles;
    }

    private List<Tile> GetAvailableCastleTiles(Pawn activePawn)
    {
        return GridGenerator.Instance.CastleTiles.Where(tile =>
         activePawn.HasPathToTile(tile) &&
         tile.GetPawn() == null).ToList();
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
                if (!activePawn.HasPathToTile(potentialTargetTile) ||
                    !potentialTargetTile.IsTraversableByThisPawn(activePawn))
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
