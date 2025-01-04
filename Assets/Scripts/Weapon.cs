using UnityEngine;
using System.Collections.Generic;

public class Weapon : MonoBehaviour
{
    private WeaponItemData _data;

    private List<Ability> _abilities = new();

    public void SetData(WeaponItemData data)
    {
        _data = data;

        StandardAttack stndAtk = new StandardAttack(data.weaponType);
        _abilities.Add(stndAtk);

        switch (data.weaponType)
        {
            case WeaponItemData.WeaponType.Club:
                break;
            case WeaponItemData.WeaponType.Sword:
                break;
            case WeaponItemData.WeaponType.Spear:
                break;
            case WeaponItemData.WeaponType.Axe:
                break;
        }
    }

}
