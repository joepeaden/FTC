using UnityEngine;
using System.Collections;

/// <summary>
/// For different beards, eyebrows, etc. Face information w/o gameplay implications
/// </summary>
[CreateAssetMenu(fileName = "FaceDetailData", menuName = "MyScriptables/FaceDetailData")]
public class FaceDetailData : ScriptableObject
{
    public enum HairColor
    {
        Brown,
        Blonde
    }

    public HairColor hairColor;
    public Sprite SWSprite;
}