using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Linq;
using UnityEngine.Events;
using Unity.VisualScripting;

public class Pawn : MonoBehaviour
{
    private const int MOT_REGAIN_RATE = 10;
    private const int MOT_BASIC_ATTACK_COST = 10;
    public const int BASE_ACTION_POINTS = 6;
    public const int MOTIVATED_MOT_REGAIN_BUFF = 15;

    public UnityEvent OnMoved = new();
    public UnityEvent OnHit = new();
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

    public int MoveRange => ActionPoints / _gameChar.GetAPPerTileMoved();

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

    public int ActionPoints;

    public int MaxMotivation => GameChar.GetBattleMotivationCap();
    public int Motivation;

    public int Initiative => GetInit();

    public bool IsDead => _isDead;
    private bool _isDead;

    public bool EngagedInCombat => GetAdjacentEnemies().Count > 0;

    public bool PendingLevelUp { get; set; }

    #region Buffs / Debuffs
    public float DodgeMod;
    public float HitMod;
    #endregion

    [SerializeField] AIPathCustom pathfinder;

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

    private bool _isMoving;
    private Vector3 _lastPosition;

    // if a pawn is guarding this pawn using an ability for example then this
    // is the reference for that guy. This is set by the HonorProtect class when
    // the ability is used.
    public Pawn ProtectingPawn { get; set; }

    [Header("Audio")]
    [SerializeField] private AudioClip _movingSound;

    /// <summary>
    /// Motivation conditions that have been fulfilled during this battle.
    /// </summary>
    private HashSet<MotCondData> _fulfilledBattleMotConds = new();

    public int BattleKills = 0;
    public int DmgInflicted = 0;

    #region UnityEvents

    private void Awake()
    {
        pathfinder.OnDestinationReached.AddListener(HandleDestinationReached);
    }

    private void Start()
    {
        PickStartTile();
        _spriteController.SetData(this);
    }

    private void OnDestroy()
    {
        pathfinder.OnDestinationReached.RemoveListener(HandleDestinationReached);
    }

    private void Update()
    {
        if (_isMoving)
        {
            _spriteController.UpdateFacingAndSpriteOrder(_lastPosition, transform.position, CurrentTile);
        }
        _lastPosition = transform.position;
    }

    #endregion

    public void SetCharacter(GameCharacter character)
    {
        _gameChar = character;
        SetTeam(character.OnPlayerTeam);
        _hitPoints = character.HitPoints;
        _armorPoints = character.GetTotalArmor();

        Motivation = GameChar.GetBattleMotivationCap();
        ActionPoints = BASE_ACTION_POINTS;

        // set up listeners motivation conditions
        SetupMotConds();

        int roll = Random.Range(0, 3);
        switch (roll)
        {
            case 0:
                GameChar.Passives.Add(DataLoader.passives["glasscannon"]);
                break;
            case 1:
                GameChar.Passives.Add(DataLoader.passives["accurate"]);
                break;
            case 2:
                GameChar.Passives.Add(DataLoader.passives["tank"]);
                break;

        }
        
        UpdateEffect(GameChar.Passives[0].effectDisplay, true);
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

    public int GetAPAfterAction()
    {
        if (Ability.SelectedAbility as WeaponAbilityData != null)
        {
            return ActionPoints - ((WeaponAbilityData)Ability.SelectedAbility).apCost;
        }
        else
        {
            return ActionPoints;
        }
    }

    public int GetMTAfterAction()
    {
        return Motivation - Ability.SelectedAbility.motCost;
    }

    public int GetAPAfterMove(Tile targetTile)
    {
        // will have no AP if leaving combat
        if (EngagedInCombat)
        {
            return 0;
        }

        int tileDist = _currentTile.GetTileDistance(targetTile);
        return Mathf.Max(ActionPoints - (tileDist * _gameChar.GetAPPerTileMoved()), 0);
    }

    private void UpdateMotivationResource()
    {
        Motivation = Mathf.Clamp(Motivation + MOT_REGAIN_RATE, Motivation, GameChar.GetBattleMotivationCap());
    }

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

    public int GetRollToHit(Pawn targetPawn)
    {
        int surroundingAllies = targetPawn.GetAdjacentEnemies().Count;

        // don't wanna include yourself so - 1
        int surroundHitMod = (surroundingAllies - 1) * 2;

        // all the hit mods don't work currently - need to be updated to d12
        //float abilityHitMod = HitMod - targetPawn.DodgeMod;

        int toHit = GameChar.AccRating - surroundHitMod; // + abilityHitMod + GameChar.TheWeapon.Data.baseAccMod + (Ability.SelectedAbility != null ? Ability.SelectedAbility.hitMod : 0);

        // don't allow guaranteed hit here
        if (toHit >= 12)
        {
            toHit = 11;
        }

        return toHit;
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
        _spriteController.HandleTurnEnd();
    }

    // perhaps this stuff should all be moved to the AttackAbility classes...
    public void AttackPawn(Pawn targetPawn, WeaponAbilityData currentAction)
    {
        // if this pawn has a protector, try to hit that one instead
        if (targetPawn.ProtectingPawn != null)
        {
            targetPawn = targetPawn.ProtectingPawn;
        }

        int toHit = GetRollToHit(targetPawn);
        int hitRoll = Random.Range(1, 12);

        BattleLogUI.Instance.AddLogEntry($"{GameChar.CharName} uses {currentAction.abilityName} against {targetPawn.GameChar.CharName}!");
        BattleLogUI.Instance.AddLogEntry($"Needs: {toHit}, Rolled: {hitRoll}");

        Vector2 attackDirection = transform.position - targetPawn.transform.position;
        attackDirection.Normalize();
        _spriteController.PlayAttack(attackDirection);

        if (hitRoll >= toHit)
        {
            bool targetHadArmor = targetPawn.ArmorPoints > 0;

            int critRollMod = 0;
            foreach (PassiveData passive in GameChar.Passives)
            {
                critRollMod += passive.critRollModifier;
            }

            bool isCrit = false;
            if (hitRoll >= (GameChar.CritChance - currentAction.critChanceMod + critRollMod))
            {
                isCrit = true;
            }

            targetPawn.TakeDamage(this, currentAction, isCrit);

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
        else
        {
            targetPawn.TriggerDodge();
            // BattleManager.Instance.AddTextNotification(transform.position, "Miss!");
            StartCoroutine(PlayAudioAfterDelay(0.1f, GameChar.TheWeapon.Data.missSound));
        }
    }

    public void TriggerLevelUpVisuals()
    {
        _spriteController.SetLevelUp();
    }

    public void TriggerDodge()
    {
        _spriteController.TriggerDodge();
    }

    public void TakeDamage(Pawn attackingPawn, WeaponAbilityData actionUsed, bool isCrit)
    {
        int hitPointsDmg = 0;
        int armorDmg = 0;
        GameCharacter attackingCharacter = attackingPawn.GameChar;

        //if (actionUsed.rangeForExtraDamage > 0 && CurrentTile.GetTileDistance(attackingPawn.CurrentTile) == actionUsed.rangeForExtraDamage)
        //{
        //    // this could just become critical hits later perhaps.
        //    extraDmgMult += actionUsed.extraDmgMultiplier;
        //}

        int dmgInMod = 0; 
        foreach(PassiveData passive in GameChar.Passives)
        {
            dmgInMod += passive.damageInModifier;
        }

        int attackDmg = attackingCharacter.GetWeaponDamageForAction(actionUsed);

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

        _spriteController.HandleHit(_isDead, armorHit, armorHit && _armorPoints <= 0);

        OnHit.Invoke();

        BattleManager.Instance.TriggerTextNotification(transform.position);
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
        // this here needs cleanup. I need to remove the ActionData stuff basically alltogether.
        if (theAbility != null)
        {
            WeaponAbilityData action = theAbility as WeaponAbilityData;
            if (action != null)
            {
                if (ActionPoints >= action.apCost && Motivation >= action.motCost)
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
                if (Motivation >= a.motCost && ActionPoints >= ((WeaponAbilityData)a).apCost)
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
        return ActionPoints >= _gameChar.GetAPPerTileMoved();
    }

    public void HandleActivation()
    {
        UpdateMotivationResource();

        _spriteController.HandleTurnBegin();
        ActionPoints = BASE_ACTION_POINTS;

        OnActivation.Invoke();
    }

    #region Movement

    /// <summary>
    /// Move as close as possible to the tile. If not enough AP, pick the next
    /// closest tile that there is enough AP for
    /// </summary>
    /// <param name="targetTile"></param>
    /// <returns></returns>
    public void TryMoveToTile(Tile targetTile)
    {
        if (ActionPoints < _gameChar.GetAPPerTileMoved())
        {
            return;
        }

        int tileDistance = _currentTile.GetTileDistance(targetTile);

        //Vector3 position = adjustedTargetTile.transform.position;
        pathfinder.AttemptGoToLocation(targetTile.transform.position);

        _spriteController.Move();

        // the ap adjustments may need to happen as the pawn enters each tile. May be best to
        // process things one tile at a time if implementing varying AP costs, etc. But not now.
        // if doing that later, make sure to update the pathfinder code too.

        // use whole turn to get out of combat with someone
        if (EngagedInCombat)
        {
            ActionPoints = 0;
            OnDisengage.Invoke();
        }
        else
        {
            ActionPoints -= _gameChar.GetAPPerTileMoved() * tileDistance;
        }

        _currentTile.PawnExitTile();

        _isMoving = true;

        _audioSource.clip = _movingSound;
        _audioSource.loop = true;
        _audioSource.Play();
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

        _spriteController.StopMoving();
        UpdateSpriteOnStop(true);

        BattleManager.Instance.PawnActivated(this);

        OnMoved.Invoke();

        _isMoving = false;

        _audioSource.loop = false;
        _audioSource.Stop();
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
        else
        {
            _spriteController.UpdateFacingAndSpriteOrder(_lastPosition, transform.position, CurrentTile);
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