using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
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

    private int _moveRange;

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

    public Sprite SEEyesSprite => _seEyesSprite;
    private Sprite _seEyesSprite;
    public Sprite SWEyesSprite => _swEyesSprite;
    private Sprite _swEyesSprite;

    // detail data objects - basically different sprites that make up the face
    public FaceDetailData HairDetail => _hairDetail;
    private FaceDetailData _hairDetail;
    public FaceDetailData FacialHairDetail => _facialHairDetail;
    private FaceDetailData _facialHairDetail;
    public FaceDetailData BrowDetail => _browDetail;
    private FaceDetailData _browDetail;

    public Weapon TheWeapon => _theWeapon;
    private Weapon _theWeapon = new Weapon();

    public List<PassiveData> Passives => _passives;
    private List<PassiveData> _passives = new ();

    public List<Ability> Abilities => _abilities;
    private List<Ability> _abilities = new ();

    public int Level => _level;
    private int _level;
    public int XP => _xp;
    private int _xp;
    public bool PendingLevelUp => _pendingLevelUp;
    private bool _pendingLevelUp = false;

    /// <summary>
    /// MotConds - short for motivation conditions. Oaths for example.
    /// </summary>
    public List<MotCondData> MotConds => _motConds;
    private List<MotCondData> _motConds = new ();

    private HashSet<MotCondData> _fulfilledMotConds = new();

    // long term effects on a character, like a broken oath (injuries?)
    public List<EffectData> Effects => _effects;
    private List<EffectData> _effects = new();

    public GameCharacterData Data => _data;
    private GameCharacterData _data;

    public GameCharacter(GameCharacterData charData)
    {
        _data = charData;

        if (charData.onPlayerTeam)
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
        }
        else
        {
            _charName = charData.characterTypeName;
        }

        foreach (Ability ab in charData.abilityList)
        {
            _abilities.Add(ab);
        }

        if (charData.onPlayerTeam)
        {
            SetMotivator((CharMotivators)Random.Range(0, 3));
        }

        _baseInitiative = Random.Range(charData.minInit, charData.maxInit);
        _hitPoints = Random.Range(charData.minHP, charData.maxHP);
        _moveRange = charData.moveRange;

        WeaponItemData startingWeapon = charData.defaultWeaponOptions[Random.Range(0, charData.defaultWeaponOptions.Count)];
        EquipItem(startingWeapon);

        if (charData.defaultArmorOptions.Count > 0)
        {
            ArmorItemData startingArmor = charData.defaultArmorOptions[Random.Range(0, charData.defaultArmorOptions.Count)];
            EquipItem(startingArmor);
        }

        _seEyesSprite = charData.eyesSE;
        _swEyesSprite = charData.eyesSW;
        _bodySprite = charData.shirt;
        GenerateFace();

        _onPlayerTeam = charData.onPlayerTeam;
        _accRating = Random.Range(charData.minAcc, charData.maxAcc);
        _critChance = 11;
    }

    private void GenerateFace()
    {
        // hell yeah bruther.

        Dictionary<string, FaceDetailData> hairDetails = DataLoader.hairDetail;
        _hairDetail = hairDetails.Values.ToList()[Random.Range(0, hairDetails.Count)];
        

        Dictionary<string, FaceDetailData> fHairDetails = DataLoader.facialHairDetail;
        List<FaceDetailData> hairDetailOptions = fHairDetails.Values.Where(x => x.hairColor == _hairDetail.hairColor).ToList(); 
        _facialHairDetail = hairDetailOptions[Random.Range(0, hairDetailOptions.Count)];
    
        Dictionary<string, FaceDetailData> browDetails = DataLoader.browDetail;
        hairDetailOptions.Clear();
        hairDetailOptions = browDetails.Values.Where(x => x.hairColor == _hairDetail.hairColor).ToList(); 
        _browDetail = hairDetailOptions[Random.Range(0, hairDetailOptions.Count)];
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
        //switch (Motivator)
        //{
        //    case CharMotivators.Honor:
                List<MotCondData> motCondsForBattle = GetMotCondsForBattle();
                for (int i = 0; i < motCondsForBattle.Count; i++)
                {
                    MotCondData condition = motCondsForBattle[i];
                    if (!_fulfilledConditions.Contains(condition))
                    {
                        FailOath(condition);

                        // add effect for display to alert the player
                        _effects.Add(DataLoader.effects["oathbroken"]);

                        failedSomething = true;
                    }
                }

                // if didn't fail any conditions
                if (!failedSomething)
                {
                    //add one motivation, up to the max, which is based on the level
                    //of the character.
                    _charMotivation = Mathf.Clamp(_charMotivation + 1, 0, _level + 1);

                    // take a new oath if not at max.
                    if (_motConds.Count < _level)
                    {
                        // AddNewOath();
                    }
                }

                //break;
            //default:
            //    Debug.Log("Battle End: Class not supported!");
            //    break;
        //}
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

    public void AddAcc(int change)
    {
        _accRating += change;
    }

    public void ChangeHP(int newValue)
    {
        _hitPoints = newValue;
    }

    public int GetXPToLevel()
    {
        return _xpCaps[_level];
    }

    private void SetHonorable()
    {
        // _abilities.Add(new HonorProtect());
        // AddNewOath();
    }

    private void AddNewOath()
    {
        switch (_level)
        {
            case 0:
                _motConds.Add(DataLoader.motConds["Kill1"]);
                break;
            case 1:
                _motConds.Add(DataLoader.motConds["NoRetreat"]);
                break;
            case 2:
                _motConds.Add(DataLoader.motConds["AllyNoDmg"]);
                break;
        }
    }

    private void SetMotivator(CharMotivators newMotivator)
    {
        _motivator = newMotivator;
        _charMotivation = 1;

        //switch (_motivator)
        //{
        //    case CharMotivators.Honor:
                SetHonorable();
        //        break;
        //    case CharMotivators.Glory:
        //        _abilities.Add(new WildAbandon());
        //        break;
        //    case CharMotivators.Greed:
        //        _abilities.Add(new BonusPay());
        //        break;
        //}
    }

    /// <summary>
    /// Returns true if level up
    /// </summary>
    /// <returns></returns>
    public bool AddXP(int xpToAdd)
    {
        if (!OnPlayerTeam)
        {
            // no exp for enemies!
            return false;
        }

        // for now, cap is level 3.
        if (_level >= 3)
        {
            return false;
        }

        _xp += xpToAdd;

        if (_xp >= _xpCaps[_level])
        {
            _xp -= _xpCaps[_level];
            _level++;
            _pendingLevelUp = true;

            return true;
        }

        return false;
    }

    public void SpendLevelUp()
    {
        _pendingLevelUp = false;
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
        int totalPassivesBuff = 0; 
        foreach (PassiveData passive in _passives)
        {
            totalPassivesBuff += passive.damageOutModifier;
        }

        // don't ever output less than 1 damage because, come on man
        return Mathf.Max(1, _theWeapon.Data.baseDamage + action.bonusDmg + totalPassivesBuff);
    }

    // public int GetWeaponArmorDamageForAction(WeaponAbilityData action)
    // {
        // Damage multipliers, and armor, needs to be reworked for the recent change from % system to d12 scale.

    //     return _theWeapon.Data.baseDamage;//Mathf.RoundToInt(GetWeaponDamageForAction(action) * (action.armorDamageMod + _theWeapon.Data.baseArmorDamage));
    // }

    public int GetTotalViceValue()
    {
        //int equipmentViceBonus = 0;
        //if (_helmItem != null && _helmItem.viceToMod == _motivator)
        //{
        //    equipmentViceBonus += _helmItem.viceMod;
        //}

        return _charMotivation;// + equipmentViceBonus;
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
                _theWeapon.SetData(GameManager.Instance.GameCharData.fallbackWeapon);
                break;
        }
    }

    public ItemData EquipItem(ItemData newItem)
    {
        if (newItem == null)
        {
            return null;
        }

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

    public int GetHitRollChance()
    {        
        int hitRollMod = 0;
        foreach (PassiveData p in _passives)
        {
            hitRollMod += p.hitRollModifier;
        }

        return AccRating + hitRollMod;
    }

    public int GetMoveRange()
    {
        // equipment move modifier
        int equipmentMoveMod = HelmItem != null ? HelmItem.moveMod : 0;

        // get passive modifiers
        int passiveMoveMod = 0;
        foreach (PassiveData p in _passives)
        {
            passiveMoveMod += p.moveModifier;
        }

        return _moveRange + equipmentMoveMod + passiveMoveMod;
    }

    #region Passives

    public bool RollPosessed()
    {
        int totalPosessionChanceRoll = -1;
        foreach (PassiveData p in _passives)
        {
            if (p.possessionChanceRoll > 0)
            {
                totalPosessionChanceRoll += p.possessionChanceRoll;
            }
        }

        if (totalPosessionChanceRoll < 0)
        {
            return false;
        }

        if (Random.Range(0, 13) > totalPosessionChanceRoll)
        {
            return true;
        }

        return false;
    }

    public bool DamageSelfOnMiss()
    {
        foreach (PassiveData p in _passives)
        {
            if (p.damageSelfOnMiss)
            {
                return true;
            }
        }

        return false;
    }

    public bool LimitedOneAttackPerTurn()
    {
        foreach (PassiveData p in _passives)
        {
            if (p.oneAttackPerTurn)
            {
                return true;
            }
        }

        return false;
    }

    public bool ShouldDowngradeCrits()
    {
        foreach (PassiveData p in _passives)
        {
            if (p.downgradesCrits)
            {
                return true;
            }
        }

        return false;
    }

    public bool CanDisengageFromCombat()
    {
        return _passives.Where(x => x.noRetreat).Count() == 0;
    }

    public int GetHealPerTurn()
    {
        int healPerTurn = 0;
        foreach (PassiveData p in _passives)
        {
            healPerTurn += p.selfHealPerTurn;
        }

        return healPerTurn;
    }

    public bool HasFreeAttacksPerEnemy()
    {
        return _passives.Where(x => x.freeAttacksPerEnemy).Count() > 0;
    }

    public bool ObsorbsAdjacentAllyDamage()
    {
        return _passives.Where(x => x.obsorbDmgFromAdjacentAlly).Count() > 0;
    }

#endregion

}
