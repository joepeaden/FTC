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
            activePawn.AttackPawnIfAPAvailable(adjacentPawn);
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

                // get vice condition values

                // need to figure this out
                //switch (activePawn.CurrentMotivator)
                //{
                //    case GameCharacter.Motivator.Sanctimony:

                //        // should I even do this?
                //        activePawn.GetMotivationVsTarget(targetPawn);

                //        //pawnAdvantageDict[targetPawn]
                //        break;

                //    case GameCharacter.Motivator.Avarice:
                //        break;

                //    case GameCharacter.Motivator.Vainglory:
                //        break;
                //}

                // we're going to have to considerm motivation values at each target
                // tile and pick the best one.
                int maxMotivation = int.MinValue;
                Tile bestTargetTile = null;
                foreach (Tile potentialTargetTile in targetPawn.CurrentTile.GetAdjacentTiles())
                {
                    // don't consider if someone's there
                    if (potentialTargetTile.GetPawn() != null)
                    {
                        continue;
                    }

                    int motATTile = activePawn.GetMotivationAtTile(potentialTargetTile);

                    if (motATTile > maxMotivation)
                    {
                        maxMotivation = motATTile;
                        bestTargetTile = potentialTargetTile;
                    }
                }

                // could be null if all positions around target are occupied
                if (bestTargetTile != null)
                {
                    moveRatingDict[bestTargetTile] = maxMotivation + eqAdvantageRating;
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

            //List<Tile> moveOptions = activePawn.CurrentTile.GetTilesInMoveRange();
            //Tile tileToMoveTo;

            //for (int i = 0; i < moveOptions.Count; i++)
            //{
            //    Tile t = moveOptions[i];
            //    if (t.GetPawn() != null)
            //    {
            //        moveOptions.Remove(t);
            //    }
            //}

            // for some reason sometimes the t.GetPawn null check above will
            // fail, so just keep picking random tiles till there's a free one
            //do
            //{
            //    tileToMoveTo = moveOptions[Random.Range(0, moveOptions.Count)];
            //}
            //while (tileToMoveTo.GetPawn() != null) ; 


            //activePawn.MoveToTileIfAPAvailable(tileToMoveTo);
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
