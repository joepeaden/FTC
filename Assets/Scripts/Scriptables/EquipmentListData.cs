using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "EquipmentListData", menuName = "MyScriptables/EquipmentListData")]
public class EquipmentListData : ScriptableObject
{
    public ArmorItemData lightHelm;
    public ArmorItemData medHelm;
    public ArmorItemData heavyHelm;
    public ArmorItemData greedHelm;
    public ArmorItemData honorHelm;
    public ArmorItemData gloryHelm;
    public ArmorItemData badHelm1;
    public WeaponItemData club;
    public WeaponItemData sword;
    public WeaponItemData axe;
    public WeaponItemData spear;

}
