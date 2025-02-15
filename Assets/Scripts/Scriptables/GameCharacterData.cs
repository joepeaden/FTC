using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "GameCharacterData", menuName = "MyScriptables/GameCharacterData")]
public class GameCharacterData : ScriptableObject
{
    public int minVice;
    public int maxVice;

    public WeaponItemData DefaultWeapon;

    public int startingGold;
    public int recruitPrice;

    public int minInit;
    public int maxInit;
    public int minHP;
    public int maxHP;
    public int minAcc;
    public int maxAcc;

    public Sprite blueShirt;
    public Sprite redShirt;
}
