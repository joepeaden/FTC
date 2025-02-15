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
                Ability abil = new BasicAttackAbility("Assets/Scriptables/Abilities/WeaponBased/ClubBasic.asset");
                _abilities.Add(abil);
                break;
            case WeaponItemData.WeaponType.Sword:
                abil = new BasicAttackAbility("Assets/Scriptables/Abilities/WeaponBased/SwordBasic.asset");
                _abilities.Add(abil);
                abil = new SlashAttackAbility();
                _abilities.Add(abil);
                break;
            case WeaponItemData.WeaponType.Spear:
                abil = new BasicAttackAbility("Assets/Scriptables/Abilities/WeaponBased/SpearBasic.asset");
                _abilities.Add(abil);
                abil = new BasicAttackAbility("Assets/Scriptables/Abilities/WeaponBased/SpearSpecial.asset");
                _abilities.Add(abil);
                break;
            case WeaponItemData.WeaponType.Axe:
                abil = new BasicAttackAbility("Assets/Scriptables/Abilities/WeaponBased/AxeBasic.asset");
                _abilities.Add(abil);
                abil = new BasicAttackAbility("Assets/Scriptables/Abilities/WeaponBased/AxeSpecial.asset");
                _abilities.Add(abil);
                break;
        }
    }
}
