using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEngine.Events;
using UnityEditor;

public class AIPathCustom : AIPath
{
    public const uint DEFAULT_NODE_TAG = 0;
    public const uint PLAYER_TEAM_NODE_TAG = 1;
    public const uint ENEMY_TEAM_NODE_TAG= 2;

    public UnityEvent OnDestinationReached = new ();
    private Seeker _seeker;
    private Pawn _pawn;

    private List<Vector3> _pathToFollow;
    private int _currentPathIndex;

    // only a local var that is used for tracking movement range when trying
    // to get to a destination. Not accurate if not being used.
    private int pawnMovesLeft;

    protected override void Awake()
    {
        _seeker = GetComponent<Seeker>();
        _pawn = GetComponent<Pawn>();

        // I'm not sure if all tags are enabled by default so just set them all to active first
        _seeker.traversableTags = -1;

        // traversable tags will be used to manage movement through allies / enemies
        // there's two - because setting up for potential perks or abilities that allow
        // movement through allies or enemies
        _seeker.traversableTags &= ~(1 << (int)ENEMY_TEAM_NODE_TAG); 
        _seeker.traversableTags &= ~(1 << (int)PLAYER_TEAM_NODE_TAG);
         
    }

    public int GetTraversableTagsBitmask()
    {
        return _seeker.traversableTags;
    }

    public void AttemptGoToLocation(Vector3 goalDestination)
    {
        _seeker.StartPath(transform.position, goalDestination, OnPathCalculated);
        pawnMovesLeft = _pawn.MoveRange;
    }

    private void OnPathCalculated(Path p)
    {
        _pathToFollow = new List<Vector3> (p.vectorPath);
        enabled = true;
        _currentPathIndex = 0;

        int totalTilesToMove = _pathToFollow.Count - 1;

        // if not enough AP to move the total distance to goal,
        // remove unreachable nodes
        if (totalTilesToMove > pawnMovesLeft)
        {
            Vector3 finalPosition = _pathToFollow[pawnMovesLeft];
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
            pawnMovesLeft--;
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
                || pawnMovesLeft <= 0)
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
