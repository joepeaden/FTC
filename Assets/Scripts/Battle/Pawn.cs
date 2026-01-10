using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using Pathfinding;

public class Pawn : MonoBehaviour
{
    private const int MOT_REGAIN_RATE = 10;
    private const int MOT_BASIC_ATTACK_COST = 10;
    public const int BASE_ACTION_POINTS = 2;
    public const int MOTIVATED_MOT_REGAIN_BUFF = 15;

    public UnityEvent OnMoved = new();
    public UnityEvent OnHPChanged = new();
    /// <summary>
    /// very specific here. lol. This is for the associated oath.
    /// </summary>
    public static UnityEvent<Pawn> OnTook3Damage = new();
    public UnityEvent<List<EffectData>> OnEffectUpdate = new();
    public UnityEvent OnActivation = new();
    private UnityEvent OnKillEnemy = new();
    private UnityEvent OnDisengage = new();

    public float baseDodgeChance;
    public float baseSurroundBonus;

    public int MoveRange => GameChar.GetMoveRange();

    public Tile CurrentTile => _currentTile;
    private Tile _currentTile;

    public bool OnPlayerTeam => _onPlayerTeam;
    private bool _onPlayerTeam;

    public int MaxHitPoints => _gameChar.HitPoints;
    public int HitPoints => _hitPoints;
    private int _hitPoints;

    public int MaxArmorPoints => _gameChar.GetTotalArmor();
    public int ArmorPoints => _armorPoints;
    private int _armorPoints;

    public int actionPoints;

    private bool _hasAttacked;
    private bool _hasMoved;
    private bool _isMyTurn;

    public int MaxMotivation => GameChar.GetBattleMotivationCap();
    public int Motivation;

    public int Initiative => GetInit();

    public bool IsDead => _isDead;
    private bool _isDead;

    public bool EngagedInCombat => GetAdjacentEnemies().Count > 0;

    public bool PendingLevelUp { get; set; }

    #region Buffs / Debuffs
    public int DodgeMod;
    public float HitMod;
    #endregion

    public int freeAttacksRemaining;

    [SerializeField] private AIPathCustom _pathfinder;

    [Header("Audio")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip armorHitSound;
    [SerializeField] private AudioClip dieSound;
    [SerializeField] private AudioSource _audioSource;

    public List<EffectData> CurrentEffects => currentEffects;
    private List<EffectData> currentEffects = new();

    public GameCharacter GameChar => _gameChar;
    private GameCharacter _gameChar;

    [SerializeField]
    private PawnSprite _spriteController;

    public GameCharacter.CharMotivators CurrentMotivator => _gameChar.Motivator;

    public bool IsPossessed;

    private bool _isMoving;
    private Vector3 _lastPosition;

    // if a pawn is guarding this pawn using an ability for example then this
    // is the reference for that guy. This is set by the HonorProtect class when
    // the ability is used.
    public Pawn ProtectingPawn { get; set; }

    public bool InDefensiveStance = false;

    [Header("Audio")]
    [SerializeField] private AudioClip _movingSound;

    /// <summary>
    /// Motivation conditions that have been fulfilled during this battle.
    /// </summary>
    private HashSet<MotCondData> _fulfilledBattleMotConds = new();

    public int BattleKills = 0;
    public int DmgInflicted = 0;

    public bool HoldingForAttackAnimation = false;

    #region UnityEvents

    private void Awake()
    {
        _pathfinder.OnDestinationReached.AddListener(HandleDestinationReached);
        _pathfinder.OnDestinationSet.AddListener(HandleNewDestination);
    }

    private void Start()
    {
        PickStartTile();
        _spriteController.SetData(this);
    }

    private void OnDestroy()
    {
        _pathfinder.OnDestinationReached.RemoveListener(HandleDestinationReached);
        _pathfinder.OnDestinationSet.RemoveListener(HandleNewDestination);
    }

    private void Update()
    {
        _lastPosition = transform.position;
    }

    #endregion

    private void HandleNewDestination(Vector3 newDestination)
    {
        if (_isMoving)
        {
            Tile tileAtThisLocation = GridGenerator.Instance.GetClosestTileToPosition(transform.position);
            _spriteController.UpdateFacingAndSpriteOrder(tileAtThisLocation.transform.position, newDestination, CurrentTile);
        }
    }

    public void SetCharacter(GameCharacter character)
    {
        _gameChar = character;
        SetTeam(character.OnPlayerTeam);
        _hitPoints = character.HitPoints;
        _armorPoints = character.GetTotalArmor();

        Motivation = GameChar.GetBattleMotivationCap();
        actionPoints = BASE_ACTION_POINTS;
        _hasMoved = false;
        _hasAttacked = false;

        // set up listeners motivation conditions
        SetupMotConds();
        
        foreach (PassiveData p in GameChar.Passives)
        {
            UpdateEffect(p.effectDisplay, true);
        }
    }

    private void SetupMotConds()
    {
        foreach (MotCondData motCond in GameChar.GetMotCondsForBattle())
        {
            switch (motCond.condType)
            {
                case MotCondData.ConditionType.KillOneEnemy:
                    OnKillEnemy.AddListener(FulfillEnemyKilledOath);
                    break;
                case MotCondData.ConditionType.DoNotDisengage:
                    // this one starts off fulfilled but is removed if
                    // it happens
                    _fulfilledBattleMotConds.Add(motCond);
                    OnDisengage.AddListener(BreakNoRetreatOath);
                    break;
                case MotCondData.ConditionType.AlliesCantTakeDamage:
                    // this one starts off fulfilled but is removed if
                    // it happens
                    _fulfilledBattleMotConds.Add(motCond);
                    OnTook3Damage.AddListener(HandlePawnTaken3DamageBrokenOath);
                    break;
                default:
                    Debug.Log("Unhandled condition!");
                    break;
            }

            UpdateEffect(motCond, true);
        }
    }

    private void BreakNoRetreatOath()
    {
        if (_fulfilledBattleMotConds.Contains(DataLoader.motConds["NoRetreat"]))
        {
            _fulfilledBattleMotConds.Remove(DataLoader.motConds["NoRetreat"]);

            // BattleManager.Instance.AddTextNotification(transform.position, "Oath Broken!");
        }
    }

    private void HandlePawnTaken3DamageBrokenOath(Pawn p)
    {
        if (p != this && p.OnPlayerTeam == OnPlayerTeam && _fulfilledBattleMotConds.Contains(DataLoader.motConds["AllyNoDmg"]))
        {
            _fulfilledBattleMotConds.Remove(DataLoader.motConds["AllyNoDmg"]);

            // BattleManager.Instance.AddTextNotification(transform.position, "Oath Broken!");
        }
    }

    private void FulfillEnemyKilledOath()
    {
        // ew string parameters.
        // whatever.
        _fulfilledBattleMotConds.Add(DataLoader.motConds["Kill1"]);
        // BattleManager.Instance.AddTextNotification(transform.position, "Oath Fufilled!");
    }

    /// <summary>
    /// Mostly updating the GameChar with battle results, like XP and
    /// fulfilled/failed oaths
    /// </summary>
    public void HandleBattleEnd()
    {
        GameChar.HandleBattleEnd(_fulfilledBattleMotConds);
    }

    public void SetTeam(bool onPlayerTeam)
    {
        _onPlayerTeam = onPlayerTeam;
    }

    public int GetMTAfterAction()
    {
        return Motivation - Ability.SelectedAbility.motCost;
    }

    // private void UpdateMotivationResource()
    // {
    //     Motivation = Mathf.Clamp(Motivation + MOT_REGAIN_RATE, Motivation, GameChar.GetBattleMotivationCap());
    // }

    private void PickStartTile()
    {
        Tile spawnTile;
        if (OnPlayerTeam)
        {
            do
            {
                spawnTile = GridGenerator.Instance.PlayerSpawns[Random.Range(0, GridGenerator.Instance.PlayerSpawns.Count)];
            } while (spawnTile.GetPawn() != null);
        }
        else
        {
            do
            {
                spawnTile = GridGenerator.Instance.EnemySpawns[Random.Range(0, GridGenerator.Instance.EnemySpawns.Count)];
            } while (spawnTile.GetPawn() != null);
        }

        _currentTile = spawnTile;
        spawnTile.PawnEnterTile(this);
        transform.position = spawnTile.transform.position;
    }
    
    public List<Pawn> GetAdjacentPawns()
    {
        List<Pawn> adjPawns = new();
        foreach (Tile t in _currentTile.GetAdjacentTiles())
        {
            Pawn p = t.GetPawn();
            if (p != null && !p.IsDead)
            {
                adjPawns.Add(p);
            }
        }

        return adjPawns;
    }

    public List<Pawn> GetAdjacentEnemies()
    {
        List<Pawn> adjacentEnemies = new();
        foreach (Tile t in _currentTile.GetAdjacentTiles())
        {
            Pawn p = t.GetPawn();
            if (p != null && p.OnPlayerTeam != _onPlayerTeam && !p.IsDead)
            {
                adjacentEnemies.Add(p);
            }
        }

        return adjacentEnemies;
    }

    #region Combat

    // public int GetDodge()
    // {
    //     return 10;
    // }

    public int GetRollToHit(Pawn targetPawn)
    {
        int surroundingAllies = targetPawn.GetAdjacentEnemies().Count;

        // don't wanna include yourself so - 1
        int surroundHitMod = (surroundingAllies - 1) * 2;

        // all the hit mods don't work currently - need to be updated to d12
        //float abilityHitMod = HitMod - targetPawn.DodgeMod;

        int toHit = GameChar.GetHitRollChance() - surroundHitMod + targetPawn.DodgeMod;//GetDodge(); // + abilityHitMod + GameChar.TheWeapon.Data.baseAccMod + (Ability.SelectedAbility != null ? Ability.SelectedAbility.hitMod : 0);



        // don't allow guaranteed hit here (minimum of 2+)
        if (toHit < 2)
        {
            toHit = 2;
        }

        return toHit;
    }

    public WeaponAbilityData GetBasicAttack()
    {
        if (GetWeaponAbilities().Count > 0)
        {
            return GetWeaponAbilities()[0] as WeaponAbilityData;
        }
        else
        {
            Debug.LogWarning("No weapon abilities found in GetBasicAttack method.");
            return new BasicAttackAbility();
        }
    }

    public List<Ability> GetWeaponAbilities()
    {
        return GameChar.TheWeapon.Abilities;
    }

    public List<Ability> GetAbilities()
    {
        return GameChar.GetAbilities();
    }

    public bool IsTargetInRange(Pawn targetPawn, Ability currentAction)
    {
        bool isCloseEnough = targetPawn.CurrentTile.GetTileDistance(CurrentTile) <= currentAction.range;

        if (isCloseEnough)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void HandleTurnEnded()
    {
        _isMyTurn = false;
        IsPossessed = false;
        _spriteController.HandleTurnEnd();
    }

    // perhaps this stuff should all be moved to the AttackAbility classes...
    public void AttackPawn(Pawn targetPawn, WeaponAbilityData currentAction)
    {
        if (GameChar.LimitedOneAttackPerTurn())
        {
            if (_hasAttacked)
            {
                return;
            }
            else
            {
                _hasAttacked = true;
            }
        }

        // if this pawn has a protector, try to hit that one instead
        if (targetPawn.ProtectingPawn != null)
        {
            targetPawn = targetPawn.ProtectingPawn;
        }

        int toHit = GetRollToHit(targetPawn);
        int hitRoll = Random.Range(1, 13);

        BattleLogUI.Instance.AddLogEntry($"{GameChar.CharName} uses {currentAction.abilityName} against {targetPawn.GameChar.CharName}!");
        BattleLogUI.Instance.AddLogEntry($"Needs: {toHit}, Rolled: {hitRoll}");

        Vector2 attackDirection = targetPawn.transform.position - transform.position;
        attackDirection.Normalize();
        _spriteController.PlayAttack(attackDirection);

        if (hitRoll < toHit)
        {
            targetPawn.TriggerDodge(this);
            
            // BattleManager.Instance.AddTextNotification(transform.position, "Miss!");
            StartCoroutine(PlayAudioAfterDelay(0.1f, GameChar.TheWeapon.Data.missSound));

            // if damage self upon miss and greater than half HP, damage self
            if (GameChar.DamageSelfOnMiss() && HitPoints >= (GameChar.HitPoints/2) && (HitPoints-1) > 0)
            {
                TakeDamage(this, 1, false);
            }
        }
        else if (ShieldBlocked(targetPawn, hitRoll, toHit))
        {
            targetPawn.TriggerBlock(attackDirection);
        }
        else
        {
            bool isCrit = GetIsCrit(hitRoll, currentAction);

            // need to get this before TakeDamage because after their armor will be different 
            bool targetHadArmor = targetPawn.ArmorPoints > 0;

            targetPawn.TakeDamage(this, currentAction, isCrit);

            CameraManager.Instance.ShakeCamera();

            if (targetPawn.IsDead)
            {
                StartCoroutine(PlayAudioAfterDelay(0f, GameChar.TheWeapon.Data.killSound));

                // need to just have it pending so that the BattleManager can regain control and
                // pause itself to give time for the level up animation, then it triggers the animaiton
                // via TriggerLevelUpVisuals.
                PendingLevelUp = GameChar.AddXP(1);
                BattleKills++;
                OnKillEnemy.Invoke();
            }
            else if (!targetHadArmor)
            {
                StartCoroutine(PlayAudioAfterDelay(0f, GameChar.TheWeapon.Data.hitSound));
            }
        }
    }

    private bool ShieldBlocked(Pawn targetPawn, int hitRoll, int toHit)
    {
        if (!targetPawn.GameChar.HasShield())
        {
            return false;
        }

        // if we were missed by the blockRange or less, then
        // the shield blocks it.

        ShieldItemData shield = targetPawn.GameChar.ShieldItem;
        int difference = Mathf.Abs(toHit - hitRoll);
        if (difference <= shield.blockRange)
        {
            return true;
        }

        return false;
    }

    private bool GetIsCrit(int hitRoll, WeaponAbilityData currentAction)
    {
        int critRollMod = 0;
        foreach (PassiveData passive in GameChar.Passives)
        {
            critRollMod += passive.critRollModifier;
        }

        // Note - the defending pawn in TakeDamage can turn this crit false if they have abilities that do so.
        if (hitRoll >= (GameChar.CritChance - currentAction.critChanceMod + critRollMod))
        {
            return true;
        }
    
        return false;
    }

    public void TriggerLevelUpVisuals()
    {
        _spriteController.SetLevelUp();
    }

    public void TriggerBlock(Vector3 attackDirection)
    {
        _spriteController.TriggerBlock(attackDirection);
        
        BattleManager.Instance.AddPendingTextNotification("Block!", Color.white);
        BattleManager.Instance.TriggerTextNotification(this.transform.position);

        StartCoroutine(PlayAudioAfterDelay(0.1f, GameChar.ShieldItem.blockSound));
    }

    public void TriggerDodge(Pawn opponentPawn)
    {
        StartCoroutine(TriggerDodgeCoroutine(opponentPawn));
    }

    private IEnumerator TriggerDodgeCoroutine(Pawn opponentPawn)
    {

        BattleManager.Instance.AddPendingTextNotification("Dodge!", Color.white);

        if (InDefensiveStance)
        {
            opponentPawn.HoldingForAttackAnimation = true;
            yield return new WaitForSeconds(.25f);

            BattleManager.Instance.AddPendingTextNotification("Counter!", Color.white);

            AttackPawn(opponentPawn, GetBasicAttack());

            opponentPawn.HoldingForAttackAnimation = false;
        }
        else
        {
            _spriteController.TriggerDodge();
        }

        BattleManager.Instance.TriggerTextNotification(this.transform.position);
    }

    public void TakeDamage(Pawn attackingPawn, int attackDmg, bool isCrit)
    {
        int hitPointsDmg = 0;
        int armorDmg = 0;

        if (GameChar.ShouldDowngradeCrits())
        {
            isCrit = false;
        }

        int dmgInMod = 0; 
        foreach(PassiveData passive in GameChar.Passives)
        {
            dmgInMod += passive.damageInModifier;
        }

        // if this guy obsorbs damage, can't benifit from an ally obsorbing damage (I just don't feel like fixing endless loop of getting adjacents when TakeDamage is called.)
        if (!GameChar.ObsorbsAdjacentAllyDamage())
        {
            // if there's an ally adjacent that can obsorb damage, reduce damage by 1.
            List<Pawn> adjacentDmgObsorbers = GetAdjacentPawns().Where(x => x.OnPlayerTeam == _onPlayerTeam && x.GameChar.ObsorbsAdjacentAllyDamage()).ToList();
            if (adjacentDmgObsorbers.Count() > 0)
            {
                adjacentDmgObsorbers[0].TakeDamage(attackingPawn, 1, isCrit);
                attackDmg -= 1;
            }
        }

        bool armorHit = false;
        if (_armorPoints > 0)
        {
            armorDmg = attackDmg + dmgInMod;
            _armorPoints = Mathf.Max(0, _armorPoints - armorDmg);

            // crits against armor do full damage to armor and hp
            if (isCrit)
            {
                hitPointsDmg = attackDmg;
            }

            armorHit = true;
        }
        else
        {
            hitPointsDmg = attackDmg;

            // crits against HP do double damage
            if (isCrit)
            {
                hitPointsDmg *= 2;
            }
        }

        _hitPoints -= Mathf.Max(0, (hitPointsDmg + dmgInMod));

        if (armorDmg > 0)
        {
            BattleManager.Instance.AddPendingTextNotification(isCrit ? armorDmg.ToString() + "(Crit!)" : armorDmg.ToString(), Color.yellow);
        }

        if (hitPointsDmg > 0)
        {
            BattleManager.Instance.AddPendingTextNotification(isCrit ? hitPointsDmg.ToString() + "(Crit!)" : hitPointsDmg.ToString(), Color.red);
        }

        if (hitPointsDmg == 0 && armorDmg == 0)
        {
            BattleManager.Instance.AddPendingTextNotification("0", Color.white);
        }
        
        attackingPawn.DmgInflicted += hitPointsDmg + armorDmg;

        // if took 3 damage, alert via event in case ally has associated oath
        if (_hitPoints <= _gameChar.HitPoints - 3)
        {
            OnTook3Damage.Invoke(this);
        }

        if (_hitPoints <= 0)
        {
            Die();
        }
        else
        {
            if (armorHit)
            {
                StartCoroutine(PlayAudioAfterDelay(0f, armorHitSound));

            }
            else
            {
                StartCoroutine(PlayAudioAfterDelay(0f, hitSound));
            }
        }


        Vector2 attackDirection = transform.position - attackingPawn.transform.position;
        _spriteController.HandleHit(attackDirection, _isDead, armorHit, armorHit && _armorPoints <= 0);

        OnHPChanged.Invoke();

        BattleManager.Instance.TriggerTextNotification(transform.position);
    }

    public void TakeDamage(Pawn attackingPawn, WeaponAbilityData actionUsed, bool isCrit)
    {
        GameCharacter attackingCharacter = attackingPawn.GameChar;

        //if (actionUsed.rangeForExtraDamage > 0 && CurrentTile.GetTileDistance(attackingPawn.CurrentTile) == actionUsed.rangeForExtraDamage)
        //{
        //    // this could just become critical hits later perhaps.
        //    extraDmgMult += actionUsed.extraDmgMultiplier;
        //}

        int attackDmg = attackingCharacter.GetWeaponDamageForAction(actionUsed);

        TakeDamage(attackingPawn, attackDmg, isCrit);
    }

    private void Die()
    {
        _spriteController.Die();
        StartCoroutine(PlayAudioAfterDelay(0f, dieSound));

        _isDead = true;

        CurrentTile.PawnExitTile();
        BattleManager.Instance.PawnKilled(this);
    }

    #endregion

    #region FX



    private IEnumerator PlayAudioAfterDelay(float delay, AudioClip clip)
    {
        yield return new WaitForSeconds(delay);

        _audioSource.clip = clip;
        _audioSource.Play();
    }

    #endregion

    public void UpdateEffect(EffectData statusEffect, bool isAdding)
    {
        if (isAdding)
        {
            currentEffects.Add(statusEffect);
        }
        else
        {
            currentEffects.Remove(statusEffect);
        }

        OnEffectUpdate.Invoke(currentEffects);
    }

    /// <summary>
    /// Does the character have enough resources to make any action?
    /// </summary>
    /// <returns></returns>
    public bool HasResourcesForAttackAction(Ability theAbility = null)
    {
        if (theAbility != null)
        {
            WeaponAbilityData action = theAbility as WeaponAbilityData;
            if (action != null)
            {
                if (actionPoints >= action.apCost && Motivation >= action.motCost)
                {
                    return true;
                }
            }
            else if(Motivation >= theAbility.motCost)
            {
                return true;
            }
            
            return false;            
        }
        else
        {
            foreach (Ability a in GameChar.TheWeapon.Abilities)
            {
                if (Motivation >= a.motCost && actionPoints >= ((WeaponAbilityData)a).apCost)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public bool HasActionsRemaining()
    {
        if (EngagedInCombat && HasResourcesForAttackAction())
        {
            return true;
        }
        
        // can still move
        if (!EngagedInCombat && HasMovesLeft())
        {
            return true;
        }

        return false;
    }

    public bool HasMovesLeft()
    {
        return !_hasMoved && actionPoints > 0;
    }

    public void HandleTurnBegin()
    {
        // UpdateMotivationResource();

        _isMyTurn = true;

        _spriteController.HandleTurnBegin();
        actionPoints = BASE_ACTION_POINTS;
        _hasMoved = false;
        _hasAttacked = false;

        if (_hitPoints < GameChar.HitPoints)
        {
            // self heal
            int healAmount = GameChar.GetHealPerTurn();
            if (healAmount > 0)
            {
                Heal(healAmount);
            }
        }

        UpdateFreeAttacksPassive();

        OnActivation.Invoke();
    }

    public void Heal(int healAmount)
    {
        // cap the max hitpoints to heal
        _hitPoints = Mathf.Min(healAmount + _hitPoints, GameChar.HitPoints);
        BattleManager.Instance.AddPendingTextNotification(healAmount.ToString(), Color.green);
        BattleManager.Instance.TriggerTextNotification(transform.position);
        OnHPChanged.Invoke();
    }

    #region Movement

    /// <summary>
    /// Move as close as possible to the tile. If not enough AP, pick the next
    /// closest tile that there is enough AP for
    /// </summary>
    /// <param name="targetTile"></param>
    /// <returns></returns>
    public void ForceMoveToTile(Tile targetTile)
    {
        _pathfinder.AttemptGoToLocation(targetTile.transform.position);

        _spriteController.Move();

        // use whole turn to get out of combat with someone
        if (EngagedInCombat)
        {
            OnDisengage.Invoke();
        }

        _currentTile.PawnExitTile();

        _isMoving = true;
    }

    public int GetPathfinderTraversableTagsBitmask()
    {
        return _pathfinder.GetTraversableTagsBitmask();
    }

    /// <summary>
    /// Move as close as possible to the tile. If not enough AP, pick the next
    /// closest tile that there is enough AP for
    /// </summary>
    /// <param name="targetTile"></param>
    /// <returns></returns>
    public void TryMoveToTile(Tile targetTile)
    {
        if (!HasMovesLeft()
            || EngagedInCombat && !GameChar.CanDisengageFromCombat())
        {
            return;
        }

        // the ap adjustments may need to happen as the pawn enters each tile. May be best to
        // process things one tile at a time if implementing varying AP costs, etc. But not now.
        // if doing that later, make sure to update the pathfinder code too.

        actionPoints -= 1;

        StartCoroutine(TryMoveToTileCoroutine(targetTile));
    }

    public IEnumerator TryMoveToTileCoroutine(Tile targetTile)
    {
        // opportunity attacks implementation
        bool gotHitByOpportunityAttack = false;
        if (EngagedInCombat)
        {
            foreach (Pawn enemyPawn in GetAdjacentEnemies())
            {
                HoldingForAttackAnimation = true;
                yield return new WaitForSeconds(.25f);

                int originalHP = _hitPoints;
                int originalArmor = _armorPoints; 
                enemyPawn.AttackPawn(this, enemyPawn.GetBasicAttack());
                HoldingForAttackAnimation = false;

                // if got hit, end the move.
                if (originalHP != _hitPoints || originalArmor != _armorPoints)
                {
                    gotHitByOpportunityAttack = true;
                    HandleDestinationReached();
                    break;
                }
            }
        }

        // don't proceed with move if got hit during opportunity attack
        if (!gotHitByOpportunityAttack)
        {
            _hasMoved = true;

            int tileDistance = _currentTile.GetTileDistance(targetTile);

            //Vector3 position = adjustedTargetTile.transform.position;
            _pathfinder.AttemptGoToLocation(targetTile.transform.position);

            _spriteController.Move();

            // use whole turn to get out of combat with someone
            if (EngagedInCombat)
            {
                OnDisengage.Invoke();
            }

            _currentTile.PawnExitTile();

            _isMoving = true;

            _audioSource.clip = _movingSound;
            _audioSource.loop = true;
            _audioSource.Play();
        }
    }

    public void PassTurn()
    {
        BattleManager.Instance.PawnActivated(this);
    }

    public void HandleDestinationReached()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, -Vector3.forward);

        foreach (RaycastHit2D hit in hits)
        {
            Tile newTile = hit.transform.GetComponent<Tile>();
            if (newTile != null)
            {
                newTile.PawnEnterTile(this);
                _currentTile = newTile;
                //pathfinder.enabled = false;
                break;
            }
        }

        if (_isMyTurn)
        {
            UpdateSpriteOnStop(true);
            BattleManager.Instance.PawnActivated(this);
            _audioSource.loop = false;
            _audioSource.Stop();
        }

        _spriteController.StopMoving();
        OnMoved.Invoke();
        UpdateFreeAttacksPassive();
        _isMoving = false;
    }

    public void UpdateFreeAttacksPassive()
    {
        foreach (PassiveData p in GameChar.Passives)
        {
            if (p.freeAttacksPerEnemy)
            {
                freeAttacksRemaining = GetAdjacentEnemies().Count();
            }
        }
    }

    public void SetSpriteFacing(Vector3 targetPos)
    {
        _spriteController.UpdateFacingAndSpriteOrder(transform.position, targetPos, CurrentTile);
    }

    public void UpdateSpriteOnStop(bool isFirst)
    {
        // Face adjacent enemies
        List<Pawn> adjEnemies = GetAdjacentEnemies();
        if (adjEnemies.Count > 0)
        {
            _spriteController.UpdateFacingAndSpriteOrder(transform.position, adjEnemies[0].transform.position, CurrentTile);
        }
        
        // get adjacent enemies to face me 
        if (isFirst)
        {
            for (int i = 0; i < adjEnemies.Count; i++)
            {
                adjEnemies[i].UpdateSpriteOnStop(false);
            }
        }
    }

    #endregion

    #region Motivation

    public int GetInit()
    {
        int baseInit = GameChar.BaseInitiative;
        int helmInit = ArmorPoints > 0 ? GameChar.HelmItem.initMod : 0;

        return baseInit + helmInit;
    }

    #endregion
}