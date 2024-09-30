using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class EnemyAI : MonoBehaviour
{
    private List<Pawn> _pawns = new();

    public void RegisterPawn(Pawn p)
    {
        _pawns.Add(p);
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

    private IEnumerator DoTurnCoroutine(Pawn activePawn)
    {
        // just add a pause so it's not jarring how fast turns change
        yield return new WaitForSeconds(1f);

        // see if there's anyone adjacent to attack, if so, attack
        Pawn adjacentPawn = GetAdjacentTarget(activePawn);
        if (adjacentPawn != null)
        {
            activePawn.AttackPawnIfResourcesAvailable(adjacentPawn);
        }
        // add comment
        else
        {
            // dictionary is (tile, advantagerating)  
            Dictionary<Tile, int> moveRatingDict = new();

            int activePawnEqRating = (activePawn.GameChar.GetTotalArmor() + activePawn.GameChar.WeaponItem.damage);
            foreach (Pawn targetPawn in BattleManager.Instance.PlayerPawns)
            {
                // get equipment advantage values
                int targetEqRating = (targetPawn.GameChar.GetTotalArmor() + targetPawn.GameChar.WeaponItem.damage);
                int eqAdvantageRating = activePawnEqRating - targetEqRating;

                Tile bestTargetTile = null;
                foreach (Tile potentialTargetTile in targetPawn.CurrentTile.GetAdjacentTiles())
                {
                    // don't consider if someone's there
                    if (potentialTargetTile.GetPawn() != null)
                    {
                        continue;
                    }

                    // just go with the first one for now
                    bestTargetTile = potentialTargetTile;
                    break;
                }

                // could be null if all positions around target are occupied
                if (bestTargetTile != null)
                {
                    moveRatingDict[bestTargetTile] = eqAdvantageRating;
                }
            }

            int bestOdds = int.MinValue;
            Tile bestTile = activePawn.CurrentTile;
            foreach (Tile t in moveRatingDict.Keys)
            {
                if (bestOdds < moveRatingDict[t])
                {
                    bestOdds = moveRatingDict[t];
                    bestTile = t;
                }
            }

            activePawn.TryMoveToTile(bestTile);
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
            if (targetPawn != null && targetPawn.OnPlayerTeam)
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
