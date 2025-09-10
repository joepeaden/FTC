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



            List<Pawn> pawnsToTarget = activePawn.OnPlayerTeam ? _enemyPawns : BattleManager.Instance.PlayerPawns;

            List<Tile> potentialTargetTiles = new();
            foreach (Pawn targetPawn in pawnsToTarget)
            {
                // don't circle around dead pawns forever (don't be fucking creepy)
                if (targetPawn.IsDead)
                {
                    continue;
                }

                Tile bestTargetTile = null;
                foreach (Tile potentialTargetTile in targetPawn.CurrentTile.GetAdjacentTiles())
                {
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
