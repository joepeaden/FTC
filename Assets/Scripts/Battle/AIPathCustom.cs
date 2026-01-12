using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEngine.Events;
using System.Linq;

public class AIPathCustom : AILerp
{
    public const uint DEFAULT_NODE_TAG = 0;
    public const uint PLAYER_TEAM_NODE_TAG = 1;
    public const uint ENEMY_TEAM_NODE_TAG= 2;

    public UnityEvent<Vector3> OnDestinationSet = new ();
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

    public bool HasPathToLocation(Tile targetTile)
    {
        return GetPathToTile(targetTile).vectorPath.Count > 0;
    }
    
    private Path GetPathToTile(Tile targetTile)
    {
        Path p = _seeker.StartPath (transform.position, targetTile.transform.position);
        p.BlockUntilCalculated();

        return p;
    }

    /// <summary>
    /// Make sure you check if it's possible to go to this destination first!
    /// </summary>
    public void AttemptGoToLocation(Tile targetTile)
    {
        Vector3 goalDestination = targetTile.transform.position;
        
        // if there's no path - just end the move for now
        if (!HasPathToLocation(targetTile))
        {
            Debug.Log("Cannot reach location - ending move!");
            SetDestinationReached();
        }

        pawnMovesLeft = _pawn.MoveRange;
        _seeker.StartPath(transform.position, goalDestination, OnPathCalculated);
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
        if (_pathToFollow.Count <= _currentPathIndex)
        {
            Debug.Log("MovewToNextPathNode: Not enough nodes in path!");
            SetDestinationReached();
            return;
        }

        destination = _pathToFollow[_currentPathIndex];
        OnDestinationSet.Invoke(destination);

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

            if (_currentPathIndex >= _pathToFollow.Count
                || pawnMovesLeft <= 0)
            {
                SetDestinationReached();
            }
            else
            {
                MoveToNextPathNode();
            }
        }
    }
    
    public Tile GetTileEndpointWithinRange(Tile targetTile)
    {
        List<Tile> t = GetPathToTileInRange(targetTile);
        return t.LastOrDefault();
    }
     
    public List<Tile> GetPathToTileInRange(Tile targetTile)
    {
        Path p = GetPathToTile(targetTile);

        List<Tile> tilePath = new();

        // now that we have path, eliminate vectors not in range
        for (int i = p.vectorPath.Count - 1; i >= 0; i--)
        {
            Vector3 position = p.vectorPath[i];

            Tile tileAtPos = GridGenerator.Instance.GetClosestTileToPosition(position);
            if (!_pawn.IsTileInMoveRange(tileAtPos))
            {
                p.vectorPath.Remove(position);
            }
            else
            {
                tilePath.Add(tileAtPos);
            }
        }

        // we went back to front. so flip them so it's origin -> destination
        tilePath.Reverse();

        return tilePath;
    }

    private void SetDestinationReached()
    {
        OnDestinationReached.Invoke();
        enabled = false;
    }

    private void OnDestroy()
    {
        OnDestinationReached.RemoveAllListeners();
    }
}
