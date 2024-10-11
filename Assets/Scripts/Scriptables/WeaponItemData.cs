using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WeaponItemData", menuName = "MyScriptables/WeaponItemData")]
public class WeaponItemData : ItemData
{
    public int baseDamage;
    public float baseArmorDamage;
    public float basePenetrationDamage;
    public float baseAccMod;
    public ActionData baseAction;
    public ActionData specialAction;
    public AudioClip hitSound;
    public AudioClip killSound;
    public AudioClip missSound;
}