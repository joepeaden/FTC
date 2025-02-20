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
    private int _charMotivation;

    public int BaseInitiative => _baseInitiative;
    private int _baseInitiative;

    public int HitPoints => _hitPoints;
    private int _hitPoints;

    public int AccRating => _accRating;
    private int _accRating;

    public int CritChance => _critChance;
    private int _critChance;

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

    // detail data objects - basically different sprites that make up the face
    public FaceDetailData HairDetail => _hairDetail;
    private FaceDetailData _hairDetail;
    public FaceDetailData FacialHairDetail => _facialHairDetail;
    private FaceDetailData _facialHairDetail;
    public FaceDetailData BrowDetail => _browDetail;
    private FaceDetailData _browDetail;

    public Weapon TheWeapon => _theWeapon;
    private Weapon _theWeapon = new Weapon();

    public List<Ability> Abilities => _abilities;
    private List<Ability> _abilities = new ();

    public int Level => _level;
    private int _level;
    public int XP => _xp;
    private int _xp;
    public int PendingStatPoints => _pendingStatPoints;
    private int _pendingStatPoints;

    /// <summary>
    /// MotConds - short for motivation conditions. Oaths for example.
    /// </summary>
    public List<MotCondData> MotConds => _motConds;
    private List<MotCondData> _motConds = new ();

    private HashSet<MotCondData> _fulfilledMotConds = new();

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

            SetMotivator((CharMotivators)Random.Range(0, 3));

            _baseInitiative = Random.Range(GameManager.Instance.GameCharData.minInit, GameManager.Instance.GameCharData.maxInit);
            _hitPoints = Random.Range(GameManager.Instance.GameCharData.minHP, GameManager.Instance.GameCharData.maxHP);
        }
        else
        {
            SetMotivator((CharMotivators)Random.Range(0, 3));

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

            GenerateFace();
        }

        _onPlayerTeam = onPlayerTeam;
        _accRating = Random.Range(GameManager.Instance.GameCharData.minAcc, GameManager.Instance.GameCharData.maxAcc);
        _critChance = 11;

        _motConds.Add(DataLoader.motConds["Kill1"]);
    }

    private void GenerateFace()
    {
        // hell yeah bruther.

        Dictionary<string, FaceDetailData> hairDetails = DataLoader.hairDetail;
        Dictionary<string, FaceDetailData> fHairDetails = DataLoader.facialHairDetail;
        Dictionary<string, FaceDetailData> browDetails = DataLoader.browDetail;

        _hairDetail = hairDetails.Values.ToList()[Random.Range(0, hairDetails.Count)];
        _facialHairDetail = fHairDetails.Values.ToList()[Random.Range(0, fHairDetails.Count)];
        _browDetail = browDetails.Values.ToList()[Random.Range(0, browDetails.Count)];
    }

    /// <summary>
    /// Get the motivation conditions that are appropriate to be fulfilled
    /// during battle (Oaths, but not equipment Desires, for example, or
    /// previously achieved Feats/Achievements)
    /// </summary>
    public List<MotCondData> GetMotCondsForBattle()
    {
        switch(_motivator)
        {
            case CharMotivators.Honor:
                return _motConds;
            default:
                Debug.Log("Class not yet supported!");
                return _motConds;
        }    
    }

    public void HandleBattleEnd(HashSet<MotCondData> _fulfilledConditions)
    {
        // +1 XP for just participating in the battle
        AddXP(1);

        bool failedSomething = false;
        // update motivation conditions being fulfilled or not if necessary
        switch (Motivator)
        {
            case CharMotivators.Honor:
                foreach (MotCondData condition in _fulfilledConditions)
                {
                    if (!GetMotCondsForBattle().Contains(condition))
                    {
                        FailOath(condition);
                        failedSomething = true;
                    }
                }
                break;
        }

        // if didn't fail any conditions, then add one motivation, up to the
        // max, which is based on the level of the character.
        if (!failedSomething)
        {
            _charMotivation += Mathf.Clamp(_charMotivation + 1, 0, _level+1);
        }
    }

    private void FailOath(MotCondData failedCondition)
    {
        // will need to implement something here to alert the player of the failure
        // but for now, just immediately dish out consequences.

        // remove the failed condition, and all conditions of greater tier.
        _motConds.Remove(failedCondition);
        foreach (MotCondData condition in _motConds)
        {
            if (failedCondition.tier <= condition.tier)
            {
                _motConds.Remove(condition);
            }    
        }

        //zero out the motivation - ouch
        _charMotivation = 0;
    }

    public void ChangeAccRating(int change)
    {
        _accRating += change;
    }

    public void ChangeHP(int change)
    {
        _hitPoints += change;
    }

    public int GetXPToLevel()
    {
        return _xpCaps[_level];
    }

    private void SetMotivator(CharMotivators newMotivator)
    {
        _motivator = newMotivator;
        _charMotivation = 1;

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

        if (_xp >= _xpCaps[_level])
        {
            _xp -= _xpCaps[_level];
            _level++;
            _pendingStatPoints++;
            return true;
        }

        return false;
    }

    public void SpendStatPoint()
    {
        _pendingStatPoints--;
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

    public int GetWeaponDamageForAction(WeaponAbilityData action)
    {
        // Damage multipliers, and armor, needs to be reworked for the recent change from % system to d12 scale.

        return _theWeapon.Data.baseDamage;//action.outDmgMod + _theWeapon.Data.baseDamage;
    }

    public int GetWeaponArmorDamageForAction(WeaponAbilityData action)
    {
        // Damage multipliers, and armor, needs to be reworked for the recent change from % system to d12 scale.

        return _theWeapon.Data.baseDamage;//Mathf.RoundToInt(GetWeaponDamageForAction(action) * (action.armorDamageMod + _theWeapon.Data.baseArmorDamage));
    }

    public int GetTotalViceValue()
    {
        int equipmentViceBonus = 0;
        if (_helmItem != null && _helmItem.viceToMod == _motivator)
        {
            equipmentViceBonus += _helmItem.viceMod;
        }

        return _charMotivation + equipmentViceBonus;
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
