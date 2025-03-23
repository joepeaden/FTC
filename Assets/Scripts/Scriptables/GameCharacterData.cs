using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "GameCharacterData", menuName = "MyScriptables/GameCharacterData")]
public class GameCharacterData : ScriptableObject
{
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
    public Sprite goodEyesSE;
    public Sprite goodEyesSW;
    public Sprite badEyesSE;
    public Sprite badEyesSW;
}
