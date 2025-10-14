# Attack Pattern System - Usage Guide

## Overview

The Attack Pattern System provides a flexible, extensible way to define different attack behaviors for weapons in the game. It allows you to create weapons that attack in different patterns based on the pawn's facing direction.

## Available Attack Patterns

### 1. **Radial Attack Pattern**
- **Description**: Attacks all tiles within range, regardless of facing direction
- **Use Case**: Traditional circular/radial attacks, omnidirectional weapons
- **Example**: A mace that can swing in any direction

### 2. **StraightLine Attack Pattern**
- **Description**: Attacks only in a straight diagonal line in the facing direction
- **Use Case**: Spears, lances, or piercing weapons that attack in a line
- **Example**: A pike that attacks directly in front of the wielder
- **Visual Pattern** (facing NE):
  ```
       X    (2 tiles away in NE direction)
     X      (1 tile away in NE direction)
   P        (Pawn position)
  ```

### 3. **LSweep Attack Pattern**
- **Description**: Attacks in an L-shape extending from the facing direction
- **Use Case**: Sweeping attacks, scythes, or weapons with extended reach in two perpendicular directions
- **Example**: A scythe that can hit tiles north, east, and northeast when facing NE
- **Visual Pattern** (facing NE):
  ```
   X X X    (North arm)
   X X      (Diagonal corner)
   P X X    (East arm, where P is pawn position)
  ```

### 4. **Cone Attack Pattern**
- **Description**: Attacks all tiles in a wide cone/quadrant in front of the pawn
- **Use Case**: Default melee attacks, wide-swing weapons, area attacks
- **Example**: A sword that can hit enemies in a forward arc
- **Visual Pattern** (facing NE):
  ```
   X X X X  (All tiles in NE quadrant)
   X X X X
   X X X
   P X
  ```

## How to Use

### Step 1: Set Attack Style in Unity Editor

1. Select your `WeaponAbilityData` ScriptableObject in Unity
2. In the Inspector, find the **Attack Pattern** section
3. Choose from the dropdown:
   - `Radial` - Attacks in all directions
   - `StraightLine` - Attacks in a straight line
   - `LSweep` - Attacks in an L-shape
   - `Cone` - Attacks in a front-facing cone (default)

### Step 2: That's It!

The system automatically:
- Highlights valid attack tiles when setting facing direction
- Filters attack targets based on the chosen pattern
- Updates dynamically as the pawn changes facing direction

## Code Examples

### Creating a New Weapon with Attack Pattern

```csharp
// In Unity Editor:
// 1. Right-click in Project window
// 2. Create > MyScriptables > WeaponAbilityData
// 3. Set properties:
//    - abilityName: "Spear Thrust"
//    - range: 3
//    - attackStyle: StraightLine
```

### Using Attack Patterns in Code

The system is designed to work automatically, but if you need to manually check valid tiles:

```csharp
// Get the weapon ability
WeaponAbilityData weaponAbility = pawn.GetBasicAttack();

// Get all valid attack tiles based on pattern
HashSet<Tile> validTiles = pawn.CurrentTile.GetTilesInRange(pawn, weaponAbility);

// Check if a specific tile is valid for attack
bool canAttack = validTiles.Contains(targetTile);
```

### Highlighting Attack Range

```csharp
// The system automatically highlights tiles, but you can manually trigger it:
Tile originTile = pawn.CurrentTile;
WeaponAbilityData weapon = pawn.GetBasicAttack();

// Highlight attack range
originTile.HighlightTilesInRange(pawn, weapon, true, Tile.TileHighlightType.AttackRange);

// Clear highlights
originTile.HighlightTilesInRange(pawn, weapon, false, Tile.TileHighlightType.AttackRange);
```

## Creating Custom Attack Patterns

If you need a new attack pattern, follow these steps:

### 1. Create a New Pattern Class

```csharp
public class CustomAttackPattern : AttackPattern
{
    public override HashSet<Tile> FilterTilesByPattern(
        Tile originTile, 
        Utils.FacingDirection pawnFacing, 
        HashSet<Tile> allTilesInRange,
        int range)
    {
        var validTiles = new HashSet<Tile>();
        var origin = originTile.Coordinates;
        
        // Your custom logic here
        foreach (var tile in allTilesInRange)
        {
            // Check if tile matches your pattern criteria
            if (YourCustomLogic(origin, tile.Coordinates, pawnFacing))
            {
                validTiles.Add(tile);
            }
        }
        
        return validTiles;
    }
}
```

### 2. Add to AttackStyle Enum

In `WeaponAbilityData.cs`, add your new style:

```csharp
public enum AttackStyle
{
    Radial,
    StraightLine,
    LSweep,
    Cone,
    YourNewStyle  // Add here
}
```

### 3. Update GetAttackPattern Method

```csharp
public AttackPattern GetAttackPattern()
{
    return attackStyle switch
    {
        AttackStyle.StraightLine => new StraightLineAttackPattern(),
        AttackStyle.LSweep => new LSweepAttackPattern(),
        AttackStyle.Cone => new ConeAttackPattern(),
        AttackStyle.Radial => new RadialAttackPattern(),
        AttackStyle.YourNewStyle => new CustomAttackPattern(),  // Add here
        _ => new ConeAttackPattern()
    };
}
```

## Technical Details

### Architecture

- **Base Class**: `AttackPattern` - Abstract base with helper methods
- **Pattern Classes**: Individual implementations (StraightLine, LSweep, Cone, Radial)
- **Integration**: `WeaponAbilityData` provides the pattern, `Tile` class uses it for highlighting and range calculation

### Facing Direction System

The system uses 4 diagonal facing directions:
- **NE** (Northeast): X positive, Y positive
- **NW** (Northwest): X negative, Y positive  
- **SE** (Southeast): X positive, Y negative
- **SW** (Southwest): X negative, Y negative

### Coordinate System

- X axis: Increases to the right
- Y axis: Increases upward
- Grid-based: Tiles adjacent in cardinal and diagonal directions

## Testing Your Patterns

1. Create a test weapon with your pattern
2. Enter the SetFacing state after moving
3. Observe which tiles are highlighted
4. Verify the pattern matches your expectations for all 4 facing directions

## Performance Considerations

- Patterns are calculated on-demand when highlighting
- The base `GetTilesInRange` uses BFS for efficient range calculation
- Pattern filtering is O(n) where n is tiles in range
- For large ranges, consider optimizing your pattern logic

## Troubleshooting

**Problem**: Tiles not highlighting correctly
- Check that the weapon has a valid `AttackStyle` set
- Verify the pawn's `CurrentFacing` is being updated
- Ensure the pattern's logic correctly handles all 4 facing directions

**Problem**: Wrong tiles being highlighted
- Review your pattern's filtering logic
- Use Debug.Log to check coordinates being evaluated
- Verify facing direction is what you expect

**Problem**: Compilation errors
- Ensure `AttackPattern.cs` is in the correct folder
- Check that all enum values match between classes
- Verify Unity has recompiled after changes

## Examples by Weapon Type

| Weapon Type | Recommended Pattern | Reasoning |
|-------------|-------------------|-----------|
| Sword | Cone | Wide melee arc |
| Spear | StraightLine | Thrusting weapon |
| Axe | Cone | Sweeping attacks |
| Dagger | Cone (range 1) | Close quarters |
| Scythe | LSweep | Unique sweeping pattern |
| Mace | Radial | Can swing all directions |
| Halberd | LSweep | Reach and sweep |

## Best Practices

1. **Start with existing patterns** - Use Cone for most melee weapons
2. **Test all facing directions** - Your pattern should work correctly for NE, NW, SE, SW
3. **Keep range reasonable** - Large ranges with complex patterns can be confusing
4. **Visual clarity** - Ensure the pattern makes sense visually in-game
5. **Balance considerations** - More restrictive patterns should compensate with other benefits

## Future Enhancements

Potential improvements you could add:
- **Cross Attack Pattern**: Attacks in + shape (cardinal directions only)
- **Arc Attack Pattern**: Attacks in a specific angle arc (e.g., 90 degrees)
- **Ring Attack Pattern**: Attacks only at specific distance
- **Asymmetric Patterns**: Different ranges in different directions

---

*For questions or issues, refer to the code documentation in `AttackPattern.cs`, `WeaponAbilityData.cs`, and `Tile.cs`*
