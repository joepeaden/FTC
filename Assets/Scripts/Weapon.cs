using UnityEngine;
using System.Collections.Generic;

public class Weapon
{
    public WeaponItemData Data => _data;
    private WeaponItemData _data;

    public List<Ability> Abilities => _abilities;
    private List<Ability> _abilities = new();
    
    public void SetData(WeaponItemData data)
    {
        _data = data;
        _abilities.Clear();

        switch (data.weaponType)
        {
            case WeaponItemData.WeaponType.Club:
                Ability abil = new BasicAttackAbility("Assets/Scriptables/Abilities/WeaponBased/BasicAttackAbility.asset");
                _abilities.Add(abil);
                break;
            case WeaponItemData.WeaponType.Sword:
                abil = new BasicAttackAbility("Assets/Scriptables/Abilities/WeaponBased/SwordBasicAbility.asset");
                _abilities.Add(abil);
                abil = new SlashAttackAbility();
                _abilities.Add(abil);
                break;
            case WeaponItemData.WeaponType.Spear:
                abil = new BasicAttackAbility("Assets/Scriptables/Abilities/WeaponBased/SpearBasicAbility.asset");
                _abilities.Add(abil);
                abil = new BasicAttackAbility("Assets/Scriptables/Abilities/WeaponBased/SpearSpecialAbility.asset");
                _abilities.Add(abil);
                break;
            case WeaponItemData.WeaponType.Axe:
                abil = new BasicAttackAbility("Assets/Scriptables/Abilities/WeaponBased/AxeBasicAbility.asset");
                _abilities.Add(abil);
                abil = new BasicAttackAbility("Assets/Scriptables/Abilities/WeaponBased/BrutalChopAbility.asset");
                _abilities.Add(abil);
                break;
        }
    }
}
