using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCharacter
{
    private const int VICE_TO_MOT_MULTIPLIER = 10;

    public enum CharVices
    {
        Greed,
        Honor,
        Glory
    }

    public string CharName => _charName;
    private string _charName;
    public bool IsPlayerChar => _isPlayerChar;
    private bool _isPlayerChar;

    public CharVices Vice => _vice;
    private CharVices _vice;
    private int _charViceValue;
    
    private int _baseInitiative;

    public int HitPoints => _hitPoints;
    private int _hitPoints;

    public ArmorItemData HelmItem => _helmItem;
    private ArmorItemData _helmItem;

    public ArmorItemData BodyItem => _bodyItem;
    private ArmorItemData _bodyItem;

    public WeaponItemData WeaponItem => _weaponItem;
    private WeaponItemData _weaponItem;

    //public Sprite FaceSprite => _faceSprite;
    //private Sprite _faceSprite;

    public GameCharacter(string newName, bool isPlayerCharacter, CharVices newVice, int newViceValue)
    {
        _charName = newName;
        _isPlayerChar = isPlayerCharacter;
        _vice = newVice;
        _charViceValue = newViceValue;
        _baseInitiative = Random.Range(GameManager.Instance.GameCharData.minInit, GameManager.Instance.GameCharData.maxInit);
        _hitPoints = Random.Range(GameManager.Instance.GameCharData.minHP, GameManager.Instance.GameCharData.maxHP);

        EquipItem(GameManager.Instance.GameCharData.DefaultWeapon);
    }

    public GameCharacter()
    {
        List<string> characterNameOptions = new()
        {
            "Ealdred",
            "Cynric",
            "Leofward",
            "Eadgarth",
            "Wulfstan",
            "Hrothgar",
            "Aldhelm",
            "Eadmund",
            "Leofric",
            "Osbruh",
            "Offa",
            "Godwin",
            "Beorthric",
            "Eadmer",
            "Godric",
            "Aldhelm",
            "Wigstan",
            "Beorn",
            "Eadric",
            "Alfwold",
            "Eadred",
            "Wulfrun",
            "Wulfric",
            "Arthur"
        };

        _charName = characterNameOptions[Random.Range(0, characterNameOptions.Count)];
        _isPlayerChar = false;

        if (GameManager.Instance != null)
        {
            GameCharacterData data = GameManager.Instance.GameCharData;
            _vice = (CharVices) Random.Range(0, 3);
            _charViceValue = Random.Range(data.minVice, data.maxVice);

            _baseInitiative = Random.Range(GameManager.Instance.GameCharData.minInit, GameManager.Instance.GameCharData.maxInit);
            _hitPoints = Random.Range(GameManager.Instance.GameCharData.minHP, GameManager.Instance.GameCharData.maxHP);
        }
        else
        {
            _vice = (CharVices)Random.Range(0, 3);
            _charViceValue = Random.Range(2, 6);

            _baseInitiative = Random.Range(0, 5);
            _hitPoints = Random.Range(30, 100);

        }
        if (GameManager.Instance != null)
        {
            EquipItem(GameManager.Instance.GameCharData.DefaultWeapon);
        }
    }

    public float GetCharHitChance()
    {
        return _weaponItem.baseAccMod;
    }

    public int GetWeaponDamageForAction(ActionData action)
    {
        return action.damageMod + _weaponItem.baseDamage;
    }

    public int GetWeaponArmorDamageForAction(ActionData action)
    {
        return Mathf.RoundToInt(GetWeaponDamageForAction(action) * (action.armorDamageMod + _weaponItem.baseArmorDamage));
    }

    public int GetWeaponPenetrationDamageForAction(ActionData action)
    {
        return Mathf.RoundToInt(GetWeaponDamageForAction(action) * (action.penetrationDamageMod + _weaponItem.basePenetrationDamage));
    }

    public int GetTotalViceValue()
    {
        int equipmentViceBonus = 0;
        if (_helmItem != null && _helmItem.viceToMod == _vice)
        {
            equipmentViceBonus += _helmItem.viceMod;
        }

        return _charViceValue + equipmentViceBonus;
    }

    public int GetInitiative()
    {
        int initMod = 0;
        if (_helmItem != null)
        {
            initMod += _helmItem.initMod;
        }

        return _baseInitiative + initMod;
    }

    public int GetBattleMotivationCap()
    {
        return GetTotalViceValue() * VICE_TO_MOT_MULTIPLIER;
    }

    public int GetTotalArmor()
    {
        if (HelmItem != null)
        {
            return HelmItem.protection;
        }

        return 0;
    }

    public void UnEquipItem(ItemData item)
    {
        switch (item.itemType)
        {
            case ItemType.Helmet:
                _helmItem = null;
                break;
            case ItemType.Armor:
                _bodyItem = null;
                break;
            case ItemType.Weapon:
                _weaponItem = GameManager.Instance.GameCharData.DefaultWeapon;
                break;
        }
    }

    public ItemData EquipItem(ItemData newItem)
    {
        ItemData oldItem = null;

        switch (newItem.itemType)
        {
            case ItemType.Helmet:
                oldItem = _helmItem;
                _helmItem = (ArmorItemData) newItem;
                break;
            case ItemType.Armor:
                oldItem = _bodyItem;
                _bodyItem = (ArmorItemData) newItem;
                break;
            case ItemType.Weapon:
                oldItem = _weaponItem;
                _weaponItem = (WeaponItemData) newItem;
                break;
        }

        return oldItem;
    }

    public int GetMoveRange()
    {
        return Pawn.BASE_ACTION_POINTS/GetAPPerTileMoved();
    }

    public int GetAPPerTileMoved()
    {
        int totalArmorAPMod = HelmItem != null ? -HelmItem.moveMod : 0;

        // so if negative then it will add AP.
        return Tile.BASE_AP_TO_TRAVERSE + totalArmorAPMod;
    }
}
