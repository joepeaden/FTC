using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEngine.Events;

public class AIPathCustom : AIPath
{
    public UnityEvent OnDestinationReached = new ();
    private Seeker _seeker;
    private Pawn _pawn;

    private List<Vector3> _pathToFollow;
    private int _currentPathIndex;

    // only a local var that is used for tracking movement range when trying
    // to get to a destination. Not accurate if not being used.
    private int _pawnActionPoints;

    protected override void Awake()
    {
        _seeker = GetComponent<Seeker>();
        _pawn = GetComponent<Pawn>();
    }

    public void AttemptGoToLocation(Vector3 goalDestination)
    {
        _seeker.StartPath(transform.position, goalDestination, OnPathCalculated);
        _pawnActionPoints = _pawn.ActionPoints;
    }

    private void OnPathCalculated(Path p)
    {
        _pathToFollow = new List<Vector3> (p.vectorPath);
        enabled = true;
        _currentPathIndex = 0;


        int totalTilesToMove = _pathToFollow.Count - 1;
        int apPerTileMoved = _pawn.GameChar.GetAPPerTileMoved();

        // if not enough AP to move the total distance to goal,
        // remove unreachable nodes
        if (totalTilesToMove * apPerTileMoved > _pawnActionPoints)
        {
            Vector3 finalPosition = _pathToFollow[_pawnActionPoints / apPerTileMoved];
            int finalIndex = _pathToFollow.IndexOf(finalPosition);
            _pathToFollow.RemoveRange(finalIndex+1, (_pathToFollow.Count - (finalIndex + 1)));
        }

        // make sure there's no pawn at the final tile
        int pathIndex = _pathToFollow.Count-1;
        while (_pathToFollow.Count > 1)
        {
            Vector3 endPoint = _pathToFollow[pathIndex];
            Tile finalTile = GridGenerator.Instance.GetClosestTileToPosition(endPoint);

            if (finalTile.GetPawn() != null)
            {
                _pathToFollow.Remove(endPoint);
            }
            else
            {
                break;
            }

            pathIndex--;
        }

        MoveToNextPathNode();
    }

    private void MoveToNextPathNode()
    {
        destination = _pathToFollow[_currentPathIndex];

        if (_currentPathIndex > 0)
        {
            _pawnActionPoints -= _pawn.GameChar.GetAPPerTileMoved();
        }
    }

    protected override void Update()
    {
        base.Update();

        if ((transform.position - destination).magnitude <= 0.1f)
        {
            _currentPathIndex++;

            //bool nextTileHasPawn = false;
            //if (_currentPathIndex == _pathToFollow.Count-2)
            //{
                //RaycastHit2D[] hits = Physics2D.RaycastAll(_pathToFollow[_currentPathIndex+1], -Vector3.forward);
                //foreach (RaycastHit2D hit in hits)
                //{
                //    Tile newTile = hit.transform.GetComponent<Tile>();
                //    if (newTile != null)
                //    {
                //        nextTileHasPawn = newTile.GetPawn() != null;
                //        break;
                //    }
                //} 
            //}

            if (_currentPathIndex >= _pathToFollow.Count
                || _pawnActionPoints <= 0)
            {
                OnDestinationReached.Invoke();
                enabled = false;
            }
            else
            {
                MoveToNextPathNode();
            }
        }
    }

    private void OnDestroy()
    {
        OnDestinationReached.RemoveAllListeners();
    }
}
