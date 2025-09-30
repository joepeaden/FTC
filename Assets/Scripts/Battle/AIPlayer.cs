using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    
    private bool CanReachSpot(Vector3 start, Vector3 end)
    {
        var startNode = AstarPath.active.GetNearest(start).node;
        var endNode = AstarPath.active.GetNearest(end).node;

        // Nodes must exist and not be blocked
        if (startNode == null || endNode == null) return false;
        if (!startNode.Walkable || !endNode.Walkable) return false;

        // Check if both nodes are in the same area (A* precomputes connected areas)
        return startNode.Area == endNode.Area;
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
            // figure out who to target first

            // figure out the path to get to that target
            // go as far as possible along that path within move range


            ////// old code to figure out the one that you ahve the best "advantage" against //////

            // dictionary is (tile, advantagerating)  
            //Dictionary<Tile, int> moveRatingDict = new();

            //int activePawnEqRating = (activePawn.GameChar.GetTotalArmor() + activePawn.GameChar.GetWeaponDamageForAction(activePawn.GameChar.WeaponItem.baseAction));
            //foreach (Pawn targetPawn in BattleManager.Instance.PlayerPawns)
            //{
            //    // don't circle around dead pawns forever (don't be fucking creepy)
            //    if (targetPawn.IsDead)
            //    {
            //        continue;
            //    }

            //    // get equipment advantage values
            //    int targetEqRating = (targetPawn.GameChar.GetTotalArmor() + targetPawn.GameChar.GetWeaponDamageForAction(targetPawn.GameChar.WeaponItem.baseAction));
            //    int eqAdvantageRating = activePawnEqRating - targetEqRating;

            //    Tile bestTargetTile = null;
            //    foreach (Tile potentialTargetTile in targetPawn.CurrentTile.GetAdjacentTiles())
            //    {
            //        Pawn pawnAtTile = potentialTargetTile.GetPawn();
            //        // don't consider if someone's there
            //        if (pawnAtTile != null  && !pawnAtTile.IsDead || potentialTargetTile.IsImpassable)
            //        {
            //            continue;
            //        }

            //        // just go with the first one for now
            //        bestTargetTile = potentialTargetTile;
            //        break;
            //    }

            //    // could be null if all positions around target are occupied
            //    if (bestTargetTile != null)
            //    {
            //        moveRatingDict[bestTargetTile] = eqAdvantageRating;
            //    }
            //}

            //int bestOdds = int.MinValue;
            //Tile bestTile = activePawn.CurrentTile;
            //foreach (Tile t in moveRatingDict.Keys)
            //{
            //    if (bestOdds < moveRatingDict[t])
            //    {
            //        bestOdds = moveRatingDict[t];
            //        bestTile = t;
            //    }
            //}

            ////////////////////////////////////////////////////////////////////////////////////////////////

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
                Tile finalTargetTile = potentialTargetTiles.OrderBy(t => activePawn.CurrentTile.GetTileDistance(t)).First();

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
            // don't circle around dead pawns forever (don't be fucking creepy)
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
                if (!CanReachSpot(transform.position, potentialTargetTile.transform.position))
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
