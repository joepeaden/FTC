using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WeaponItemData", menuName = "MyScriptables/WeaponItemData")]
public class WeaponItemData : ItemData
{
    [Header("Weapon Info")]
    public int baseDamage;
    public List<Ability> abilities;

    [Header("Weapon Audio")]
    public AudioClip hitSound;
    public AudioClip killSound;
    public AudioClip missSound;
}