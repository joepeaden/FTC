﻿using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "GameCharacterData", menuName = "MyScriptables/GameCharacterData")]
public class GameCharacterData : ScriptableObject
{
    public int minVice;
    public int maxVice;

    public WeaponItemData DefaultWeapon;

    public int startingGold;
    public int recruitPrice;
}