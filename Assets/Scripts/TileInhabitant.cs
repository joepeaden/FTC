using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class TileInhabitant : MonoBehaviour
{
    public enum InhabitantType
    {
        Pawn,
        Item,
        MissionBoard
    } 
    
    public InhabitantType TheInhabitantType;
    
    public Tile CurrentTile;
}