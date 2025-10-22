using UnityEngine;
using System.Collections.Generic;

public class Weapon
{
    public WeaponItemData Data => _data;
    private WeaponItemData _data;

    public List<Ability> Abilities => _data.abilities;
    // private List<Ability> _abilities = new();

    public void SetData(WeaponItemData data)
    {
        _data = data;
    }
}
