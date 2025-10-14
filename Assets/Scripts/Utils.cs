using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public enum FacingDirection
    {
        NW,
        NE,
        SE,
        SW
    }

    public static FacingDirection GetDirection(Vector3 originPosition, Vector3 targetPosition)
    {
        if (targetPosition.x > originPosition.x && targetPosition.y > originPosition.y)
        {
            return FacingDirection.NE;
        }
        else if (targetPosition.x < originPosition.x && targetPosition.y > originPosition.y)
        {
            return FacingDirection.NW;
        }
        else if (targetPosition.x > originPosition.x && targetPosition.y < originPosition.y)
        {
            return FacingDirection.SE;
        }
        else
        {
            return FacingDirection.SW;
        }
    }
}
