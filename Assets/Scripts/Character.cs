using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Character : MonoBehaviour
{
    public int MoveRange => moveRange;
    private int moveRange = 4;

    [SerializeField] AIPathCustom pathfinder;

    private void Awake()
    {
        pathfinder.OnDestinationReached.AddListener(HandleDestinationReached);
    }

    public void GoToSpot(Vector3 position)
    {
        pathfinder.enabled = true;
        pathfinder.destination = position;
    }

    public void HandleDestinationReached()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, -Vector3.forward);

        foreach (RaycastHit2D hit in hits)
        {
            Tile newTile = hit.transform.GetComponent<Tile>();
            if (newTile != null)
            {
                newTile.CharacterEnterTile(this);
                pathfinder.enabled = false;
                break;
            }
        }
    }

    private void OnDestroy()
    {
        pathfinder.OnDestinationReached.RemoveListener(HandleDestinationReached);
    }
}