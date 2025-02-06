using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameCharacter
{
    /// <summary>
    /// Each of these corresponds to the XP cap to level up.
    /// For example, index 0 is the XP needed to get from level
    /// 0 to level 1.
    /// </summary>
    private static int[] _xpCaps = new int[]
    {
        2,
        4,
        6,
        8
    };

    public enum CharMotivators
    {
        Greed,
        Honor,
        Glory
    }

    public string CharName => _charName;
    private string _charName;

    public CharMotivators Motivator => _motivator;
    private CharMotivators _motivator;
    private int _charMotivatorValue;

    public int BaseInitiative => _baseInitiative;
    private int _baseInitiative;

    public int HitPoints => _hitPoints;
    private int _hitPoints;

    public int AccRating => _accRating;
    private int _accRating;

    public ArmorItemData HelmItem => _helmItem;
    private ArmorItemData _helmItem;

    //public ArmorItemData BodyItem => _bodyItem;
    /// <summary>
    /// this probably needs to be removed.
    /// </summary>
    private ArmorItemData _bodyItem;

    public bool OnPlayerTeam => _onPlayerTeam;
    private bool _onPlayerTeam;

    // this may need to be associated with BodyItem later. Not sure at this point.
    public Sprite BodySprite => _bodySprite;
    private Sprite _bodySprite;

    public Weapon TheWeapon => _theWeapon;
    private Weapon _theWeapon = new Weapon();

    //public Sprite FaceSprite => _faceSprite;
    //private Sprite _faceSprite;

    public List<Ability> Abilities => _abilities;
    private List<Ability> _abilities = new ();

    public int Level => _level;
    private int _level;

    public int XP => _xp;
    private int _xp;

    public GameCharacter(string newName, CharMotivators newMotivator, int newMotivatorValue, bool onPlayerTeam)
    {
        _charName = newName;

        SetMotivator(newMotivator, newMotivatorValue);

        _baseInitiative = Random.Range(GameManager.Instance.GameCharData.minInit, GameManager.Instance.GameCharData.maxInit);
        _hitPoints = Random.Range(GameManager.Instance.GameCharData.minHP, GameManager.Instance.GameCharData.maxHP);

        EquipItem(GameManager.Instance.GameCharData.DefaultWeapon);

        if (onPlayerTeam)
        {
            _bodySprite = GameManager.Instance.GameCharData.blueShirt;
        }
        else
        {
            _bodySprite = GameManager.Instance.GameCharData.redShirt;
        }

        _onPlayerTeam = onPlayerTeam;
    }

    public GameCharacter(bool onPlayerTeam)
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

        if (GameManager.Instance != null)
        {
            GameCharacterData data = GameManager.Instance.GameCharData;

            SetMotivator((CharMotivators)Random.Range(0, 3), Random.Range(data.minVice, data.maxVice));

            _baseInitiative = Random.Range(GameManager.Instance.GameCharData.minInit, GameManager.Instance.GameCharData.maxInit);
            _hitPoints = Random.Range(GameManager.Instance.GameCharData.minHP, GameManager.Instance.GameCharData.maxHP);
        }
        else
        {
            SetMotivator((CharMotivators)Random.Range(0, 3), Random.Range(2, 6));

            _baseInitiative = Random.Range(0, 5);
            _hitPoints = Random.Range(3, 8);

        }
        if (GameManager.Instance != null)
        {
            EquipItem(GameManager.Instance.GameCharData.DefaultWeapon);
            if (onPlayerTeam)
            {
                _bodySprite = GameManager.Instance.GameCharData.blueShirt;
            }
            else
            {
                _bodySprite = GameManager.Instance.GameCharData.redShirt;
            }
        }

        _onPlayerTeam = onPlayerTeam;
    }

    private void SetMotivator(CharMotivators newMotivator, int newMotivatorValue)
    {
        _motivator = newMotivator;
        _charMotivatorValue = newMotivatorValue;

        switch (_motivator)
        {
            case CharMotivators.Honor:
                _abilities.Add(new HonorProtect());
                break;
            case CharMotivators.Glory:
                _abilities.Add(new WildAbandon());
                break;
            case CharMotivators.Greed:
                _abilities.Add(new BonusPay());
                break;
        }
    }

    /// <summary>
    /// Returns true if level up
    /// </summary>
    /// <returns></returns>
    public bool AddXP(int xpToAdd)
    {
        _xp += xpToAdd;

        if (_xp >= 1)//_xpCaps[_level])
        {
            _xp -= _xpCaps[_level];
            _level++;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Return character abilities + weapon abilities
    /// </summary>
    /// <returns></returns>
    public List<Ability> GetAbilities() 
    {
        List<Ability> abilitiesToReturn = _theWeapon.Abilities;
        return abilitiesToReturn.Concat(_abilities).ToList();
    }

    public int GetWeaponDamageForAction(ActionData action)
    {
        return action.outDmgMod + _theWeapon.Data.baseDamage;
    }

    public int GetWeaponArmorDamageForAction(ActionData action)
    {
        return Mathf.RoundToInt(GetWeaponDamageForAction(action) * (action.armorDamageMod + _theWeapon.Data.baseArmorDamage));
    }

    public int GetWeaponPenetrationDamageForAction(ActionData action)
    {
        return Mathf.RoundToInt(GetWeaponDamageForAction(action) * (action.penetrationDamageMod + _theWeapon.Data.basePenetrationDamage));
    }

    public int GetTotalViceValue()
    {
        int equipmentViceBonus = 0;
        if (_helmItem != null && _helmItem.viceToMod == _motivator)
        {
            equipmentViceBonus += _helmItem.viceMod;
        }

        return _charMotivatorValue + equipmentViceBonus;
    }

    public int GetInitiativeWithEquipment()
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
        return GetTotalViceValue();
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
                _theWeapon.SetData(GameManager.Instance.GameCharData.DefaultWeapon);
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
                oldItem = _theWeapon.Data;
                _theWeapon.SetData((WeaponItemData) newItem);
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
