using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WeaponItemData", menuName = "MyScriptables/WeaponItemData")]
public class WeaponItemData : ItemData
{
    public enum WeaponType
    {
        Club,
        Sword,
        Spear,
        Axe
    }

    [Header("Weapon Info")]
    public WeaponType weaponType;
    public int baseDamage;
    public List<Ability> abilities;

    [Header("Weapon Audio")]
    public AudioClip hitSound;
    public AudioClip killSound;
    public AudioClip missSound;
}