using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "ArmorItemData", menuName = "MyScriptables/ArmorItemData")]
public class ArmorItemData : ItemData
{
    public const int MAX_ARMOR_PROTECTION = 3;

    public int protection;
}