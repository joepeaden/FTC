using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

/// <summary>
/// Base class for attack patterns that define which tiles are valid attack targets
/// based on the attacker's position and facing direction.
/// </summary>
public abstract class AttackPattern
{
    /// <summary>
    /// Filters tiles based on the attack pattern and the pawn's facing direction.
    /// </summary>
    /// <param name="originTile">The tile the attacking pawn is on</param>
    /// <param name="pawnFacing">The direction the pawn is facing</param>
    /// <param name="allTilesInRange">All tiles within range (before pattern filtering)</param>
    /// <param name="range">The maximum range of the attack</param>
    /// <returns>HashSet of tiles that are valid attack targets for this pattern</returns>
    public abstract HashSet<Tile> FilterTilesByPattern(
        Tile originTile, 
        Utils.FacingDirection pawnFacing, 
        HashSet<Tile> allTilesInRange,
        int range);

    /// <summary>
    /// Helper method to check if a tile is in a specific cardinal direction from the origin.
    /// </summary>
    protected bool IsTileInDirection(Point origin, Point target, Utils.FacingDirection direction)
    {
        int dx = target.X - origin.X;
        int dy = target.Y - origin.Y;

        switch (direction)
        {
            case Utils.FacingDirection.NE:
                return dx >= 0 && dy >= 0 && (dx > 0 || dy > 0); // At least one must be positive
            case Utils.FacingDirection.NW:
                return dx <= 0 && dy >= 0 && (dx < 0 || dy > 0);
            case Utils.FacingDirection.SE:
                return dx >= 0 && dy <= 0 && (dx > 0 || dy < 0);
            case Utils.FacingDirection.SW:
                return dx <= 0 && dy <= 0 && (dx < 0 || dy < 0);
            default:
                return false;
        }
    }

    /// <summary>
    /// Gets the primary axis for a facing direction (X or Y).
    /// </summary>
    protected (int dx, int dy) GetFacingVector(Utils.FacingDirection direction)
    {
        switch (direction)
        {
            case Utils.FacingDirection.NE: return (1, 1);
            case Utils.FacingDirection.NW: return (-1, 1);
            case Utils.FacingDirection.SE: return (1, -1);
            case Utils.FacingDirection.SW: return (-1, -1);
            default: return (0, 0);
        }
    }
}

public class StraightLineAttackPattern : AttackPattern
{
    public override HashSet<Tile> FilterTilesByPattern(
        Tile originTile, 
        Utils.FacingDirection pawnFacing, 
        HashSet<Tile> allTilesInRange,
        int range)
    {
        var validTiles = new HashSet<Tile>();

        foreach (var tile in allTilesInRange)
        {
            if (tile == originTile) continue;

            if (pawnFacing == Utils.GetDirection(originTile.transform.position, tile.transform.position))
            {
                validTiles.Add(tile);
            }
        }

        return validTiles;
    }
}

/// <summary>
/// L-shaped sweep attack pattern - attacks in an L shape extending from the facing direction.
/// Example: If facing NE, creates an L that extends north and east from the pawn's position.
/// </summary>
public class LSweepAttackPattern : AttackPattern
{
    public override HashSet<Tile> FilterTilesByPattern(
        Tile originTile, 
        Utils.FacingDirection pawnFacing, 
        HashSet<Tile> allTilesInRange,
        int range)
    {
        var validTiles = new HashSet<Tile>();

        foreach (var tile in allTilesInRange)
        {
            if (tile == originTile) continue;

            Utils.FacingDirection tileDirection = Utils.GetDirection(originTile.transform.position, tile.transform.position);
            
            // L-shape: tile is valid if it's in the same direction as facing, or perpendicular to it
            // For NE facing: accept NE (diagonal), N-aligned (NW), or E-aligned (SE)
            // For NW facing: accept NW (diagonal), N-aligned (NE), or W-aligned (SW)
            // etc.
            
            bool isInFront = tileDirection == pawnFacing;
            bool isPerpendicularRight = IsPerpendicularRightDirection(pawnFacing, tileDirection);
            
            if (isInFront || isPerpendicularRight)
            {
                validTiles.Add(tile);
            }
        }

        return validTiles;
    }

    private bool IsPerpendicularRightDirection(Utils.FacingDirection facing, Utils.FacingDirection tileDir)
    {
        // For each facing direction, determine which two directions form the perpendicular arms
        switch (facing)
        {
            case Utils.FacingDirection.NE:
                return tileDir == Utils.FacingDirection.SE;
            case Utils.FacingDirection.NW:
                return tileDir == Utils.FacingDirection.NE;
            case Utils.FacingDirection.SE:
                return tileDir == Utils.FacingDirection.SW;
            case Utils.FacingDirection.SW:
                return tileDir == Utils.FacingDirection.NW;
            default:
                return false;
        }
    }
}

/// <summary>
/// Cone attack pattern - attacks in a wide cone in front of the pawn.
/// Includes tiles in the facing direction and the two adjacent directions.
/// Example: If facing NE, attacks tiles in NE, NW, and SE directions (forming a cone).
/// </summary>
public class ConeAttackPattern : AttackPattern
{
    public override HashSet<Tile> FilterTilesByPattern(
        Tile originTile, 
        Utils.FacingDirection pawnFacing, 
        HashSet<Tile> allTilesInRange,
        int range)
    {
        var validTiles = new HashSet<Tile>();

        foreach (var tile in allTilesInRange)
        {
            if (tile == originTile) continue;

            Utils.FacingDirection tileDirection = Utils.GetDirection(originTile.transform.position, tile.transform.position);
            
            // Cone includes facing direction and two perpendicular directions
            if (tileDirection == pawnFacing || IsPerpendicularDirection(pawnFacing, tileDirection))
            {
                validTiles.Add(tile);
            }
        }

        return validTiles;
    }

    private bool IsPerpendicularDirection(Utils.FacingDirection facing, Utils.FacingDirection tileDir)
    {
        // For each facing direction, determine which two directions form the cone sides
        switch (facing)
        {
            case Utils.FacingDirection.NE:
                return tileDir == Utils.FacingDirection.NW || tileDir == Utils.FacingDirection.SE;
            case Utils.FacingDirection.NW:
                return tileDir == Utils.FacingDirection.NE || tileDir == Utils.FacingDirection.SW;
            case Utils.FacingDirection.SE:
                return tileDir == Utils.FacingDirection.NE || tileDir == Utils.FacingDirection.SW;
            case Utils.FacingDirection.SW:
                return tileDir == Utils.FacingDirection.NW || tileDir == Utils.FacingDirection.SE;
            default:
                return false;
        }
    }
}

/// <summary>
/// Standard circular/radial attack pattern - attacks all tiles within range regardless of facing.
/// This is the default pattern when no specific attack pattern is defined.
/// </summary>
public class RadialAttackPattern : AttackPattern
{
    public override HashSet<Tile> FilterTilesByPattern(
        Tile originTile,
        Utils.FacingDirection pawnFacing,
        HashSet<Tile> allTilesInRange,
        int range)
    {
        // Return all tiles - no filtering based on facing
        return allTilesInRange;
    }
}

/// <summary>
/// Area attack pattern - attacks in a 3x3 rectangular area directly in front of the pawn.
/// Creates a 3-tile wide by 3-tile deep rectangle in the facing direction.
/// </summary>
public class AreaAttackPattern : AttackPattern
{
    public override HashSet<Tile> FilterTilesByPattern(
        Tile originTile,
        Utils.FacingDirection pawnFacing,
        HashSet<Tile> allTilesInRange,
        int range)
    {
        var validTiles = new HashSet<Tile>();
        var (forwardDx, forwardDy) = GetForwardVector(pawnFacing);
        var (perpDx, perpDy) = GetPerpendicularVector(pawnFacing);

        // Create a 3-wide by 3-deep rectangle
        for (int depth = 1; depth <= 3; depth++)
        {
            for (int width = -1; width <= 1; width++)
            {
                int targetX = originTile.Coordinates.X + forwardDx * depth + perpDx * width;
                int targetY = originTile.Coordinates.Y + forwardDy * depth + perpDy * width;

                var targetPoint = new Point(targetX, targetY);

                // Check if tile exists in the grid
                if (GridGenerator.Instance.Tiles.TryGetValue(targetPoint, out Tile targetTile) &&
                    allTilesInRange.Contains(targetTile))
                {
                    validTiles.Add(targetTile);
                }
            }
        }

        return validTiles;
    }

    private (int, int) GetForwardVector(Utils.FacingDirection facing)
    {
        switch (facing)
        {
            case Utils.FacingDirection.NE:
                return (1, 0);
            case Utils.FacingDirection.NW:
                return (0, -1);
            case Utils.FacingDirection.SE:
                return (0, 1);
            case Utils.FacingDirection.SW:
                return (-1, 0);
            default:
                return (1, 0);
        }
    }

    private (int, int) GetPerpendicularVector(Utils.FacingDirection facing)
    {
        switch (facing)
        {
            case Utils.FacingDirection.NE:
                return (0, 1); // Perpendicular to NE is along Y axis
            case Utils.FacingDirection.NW:
                return (1, 0); // Perpendicular to NW is along X axis
            case Utils.FacingDirection.SE:
                return (1, 0); // Perpendicular to SE is along X axis
            case Utils.FacingDirection.SW:
                return (0, 1); // Perpendicular to SW is along Y axis
            default:
                return (0, 1);
        }
    }
}
