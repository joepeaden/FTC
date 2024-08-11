using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "WeaponItemData", menuName = "MyScriptables/WeaponItemData")]
public class WeaponItemData : ItemData
{
    public const int MAX_WEAPON_DAMAGE = 3;

    public int damage;
}