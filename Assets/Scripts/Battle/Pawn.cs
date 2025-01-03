using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Linq;
using UnityEngine.Events;

public class Pawn : MonoBehaviour
{
    private const int MOT_REGAIN_RATE = 10;
    private const int MOT_BASIC_ATTACK_COST = 10;
    public const int BASE_ACTION_POINTS = 6;
    public const int MOTIVATED_MOT_REGAIN_BUFF = 15;

    public UnityEvent OnPawnHit = new();
    private static UnityEvent UpdateMotivationEvent = new();
    public UnityEvent<List<EffectData>> OnEffectUpdate = new();

    public float baseHitChance;
    public float baseDodgeChance;
    public float baseSurroundBonus;

    public int MoveRange => _actionPoints / _gameChar.GetAPPerTileMoved();

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

    public int ActionPoints => _actionPoints;
    private int _actionPoints;

    public int MaxMotivation => GameChar.GetBattleMotivationCap();
    public int Motivation => _motivation;
    private int _motivation;

    public int Initiative => GetInit() + GetInitBuff();

    public bool IsDead => _isDead;
    private bool _isDead;

    public bool EngagedInCombat => GetAdjacentEnemies().Count > 0;

    public bool IsMotivated => _isMotivated;
    private bool _isMotivated;

    [SerializeField] AIPathCustom pathfinder;

    [Header("Audio")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip armorHitSound;
    [SerializeField] private AudioClip dieSound;
    [SerializeField] private AudioClip greedViceSound;
    [SerializeField] private AudioClip gloryViceSound;
    [SerializeField] private AudioClip honorViceSound;
    [SerializeField] private AudioSource _audioSource;

    [Header("Character Effects")]
    [SerializeField] private EffectData honorEffect;
    [SerializeField] private EffectData greedEffect;
    [SerializeField] private EffectData gloryEffect;
    public List<EffectData> CurrentEffects => currentEffects;
    private List<EffectData> currentEffects = new();

    public GameCharacter GameChar => _gameChar;
    private GameCharacter _gameChar;

    [SerializeField]
    private PawnSprite _spriteController;

    public GameCharacter.CharVices CurrentVice => _gameChar.Vice;

    private bool _hasMadeFreeAttack;

    private bool _isMoving;
    private Vector3 _lastPosition;

    #region UnityEvents

    private void Awake()
    {
        pathfinder.OnDestinationReached.AddListener(HandleDestinationReached);
        
        UpdateMotivationEvent.AddListener(UpdateMotivatedStatus);
    }

    private void Start()
    {
        PickStartTile();
        _spriteController.SetData(this);
    }

    private void OnDestroy()
    {
        pathfinder.OnDestinationReached.RemoveListener(HandleDestinationReached);
        UpdateMotivationEvent.RemoveListener(UpdateMotivatedStatus);
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

        _motivation = GameChar.GetBattleMotivationCap();
        _actionPoints = BASE_ACTION_POINTS;


        //_anim.SetInteger("Vice", (int)_gameChar.Vice);
    }

    public void SetTeam(bool onPlayerTeam)
    {
        _onPlayerTeam = onPlayerTeam;
    }

    public int GetAPAfterAction(ActionData theAction)
    {
        return ActionPoints - theAction.apCost;
    }

    public int GetMTAfterAction(ActionData theAction)
    {
        return Motivation - theAction.motCost;
    }

    public int GetAPAfterMove(Tile targetTile)
    {
        // will have no AP if leaving combat
        if (EngagedInCombat)
        {
            return 0;
        }

        int tileDist = _currentTile.GetTileDistance(targetTile);
        return Mathf.Max(_actionPoints - (tileDist * _gameChar.GetAPPerTileMoved()), -1);
    }

    //public Sprite GetFaceSprite()
    //{
        // later, can return more than just a single sprite. For example wounds, current equipment, headgear,
        // hair, etc. I'm not sure how that will work.

        //if (GameChar.FaceSprite == null)
        //{
        //return _headSpriteRend.sprite;
        //}
    //}

    private void UpdateMotivationResource()
    {
        _motivation = Mathf.Clamp(_motivation + MOT_REGAIN_RATE + (_isMotivated ? MOTIVATED_MOT_REGAIN_BUFF : 0), _motivation, GameChar.GetBattleMotivationCap());
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

    public float GetHitChance(Pawn targetPawn)
    {
        float motivationHitBonus = 0f;
        //switch (_motivation)
        //{
        //    case 1:
        //        motivationHitBonus = -0.1f;
        //        break;
        //    case 2:
        //        motivationHitBonus = -0.05f;
        //        break;
        //    case 3:
        //        motivationHitBonus = 0f;
        //        break;
        //    case 4:
        //        motivationHitBonus = 0.05f;
        //        break;
        //    case 5:
        //        motivationHitBonus = 0.1f;
        //        break;
        //    case 6:
        //        motivationHitBonus = 0.15f;
        //        break;
        //}

        int surroundingAllies = targetPawn.GetAdjacentEnemies().Count;

        // don't wanna include yourself so - 1
        float surroundHitBonus = (surroundingAllies - 1) * .05f;

        // greedy passive is increased hit bonus when surround
        if (GameChar.Vice == GameCharacter.CharVices.Greed && _isMotivated)
        {
            motivationHitBonus = surroundHitBonus;
        }

        // honor passive is big bonus to hit
        if (GameChar.Vice == GameCharacter.CharVices.Honor && _isMotivated)
        {
            motivationHitBonus = +.2f;
        }

        // honor passive is better dodge in 1v1
        if (targetPawn.GameChar.Vice == GameCharacter.CharVices.Honor && targetPawn._isMotivated)
        {
            motivationHitBonus = -.2f;
        }

        float hitChance = baseHitChance + motivationHitBonus + surroundHitBonus + GameChar.GetCharHitChance() + (BattleManager.Instance.CurrentAction != null ? BattleManager.Instance.CurrentAction.accMod : 0);
        return hitChance;
    }

    public bool IsTargetInRange(Pawn targetPawn, ActionData currentAction)
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

    public void AttackPawnIfResourcesAvailable(Pawn primaryTargetPawn)
    {
        ActionData currentAction;
        if (primaryTargetPawn._onPlayerTeam)
        {
            currentAction = GameChar.WeaponItem.baseAction;
        }
        else
        {
            currentAction = BattleManager.Instance.CurrentAction;
        }


        if (_actionPoints < currentAction.apCost || _motivation < currentAction.motCost)
        {
            BattleManager.Instance.PawnActivated(this);
            return;
        }

        List<Pawn> targetPawns = new();
        targetPawns.Add(primaryTargetPawn);

        if (currentAction.attackStyle == ActionData.AttackStyle.LShape)
        {
            Tile clockwiseNextTile = _currentTile.GetClockwiseNextTile(primaryTargetPawn.CurrentTile);
            if (clockwiseNextTile.GetPawn())
            {
                targetPawns.Add(clockwiseNextTile.GetPawn());
            }
        }

        foreach (Pawn targetPawn in targetPawns)
        {
            AttackPawn(targetPawn, currentAction);
        }

        if (GameChar.Vice == GameCharacter.CharVices.Glory && _isMotivated && !_hasMadeFreeAttack)
        {
            _hasMadeFreeAttack = true;
        }
        else
        {
            _actionPoints -= currentAction.apCost;
            _motivation -= currentAction.motCost;
        }

        BattleManager.Instance.PawnActivated(this);

        _spriteController.UpdateFacingAndSpriteOrder(transform.position, primaryTargetPawn.transform.position, CurrentTile);
    }

    public void HandleTurnEnded()
    {
        _hasMadeFreeAttack = false;

        _spriteController.HandleTurnEnd();
    }

    private void AttackPawn(Pawn targetPawn, ActionData currentAction)
    {
        float hitChance = GetHitChance(targetPawn);
        float hitRoll = Random.Range(0f, 1f);

        //BattleLogUI.Instance.AddLogEntry($"{GameChar.CharName} uses {currentAction.actionName} against {targetPawn.GameChar.CharName}!");
        //BattleLogUI.Instance.AddLogEntry($"Chance: {(int)(hitChance * 100)}, Rolled: {(int)(hitRoll * 100)}");

        Vector2 attackDirection = transform.position - targetPawn.transform.position;
        attackDirection.Normalize();

        _spriteController.PlayAttack(attackDirection);

        if (hitRoll < hitChance)
        {
            bool targetHadArmor = targetPawn.ArmorPoints > 0;

            targetPawn.TakeDamage(this, currentAction);
            BattleManager.Instance.AddTextNotification(transform.position, "Hit!");

            if (targetPawn.IsDead)
            {
                StartCoroutine(PlayAudioAfterDelay(0f, GameChar.WeaponItem.killSound));
            }
            else if (!targetHadArmor)
            {
                StartCoroutine(PlayAudioAfterDelay(0f, GameChar.WeaponItem.hitSound));
            }
        }
        else
        {
            targetPawn.TriggerDodge();
            BattleManager.Instance.AddTextNotification(transform.position, "Miss!");
            StartCoroutine(PlayAudioAfterDelay(0.1f, GameChar.WeaponItem.missSound));
        }

    }

    public void TriggerDodge()
    {
        _spriteController.TriggerDodge();
    }

    public void TakeDamage(Pawn attackingPawn, ActionData actionUsed)
    {
        int hitPointsDmg;
        GameCharacter attackingCharacter = attackingPawn.GameChar;
        float extraDmgMult = 1;

        if (actionUsed.rangeForExtraDamage > 0 && CurrentTile.GetTileDistance(attackingPawn.CurrentTile) == actionUsed.rangeForExtraDamage)
        {
            // this could just become critical hits later perhaps.
            extraDmgMult = actionUsed.extraDmgMultiplier;
        }

        bool armorHit = false;
        if (_armorPoints > 0)
        {
            _armorPoints = Mathf.Max(0, (_armorPoints - Mathf.RoundToInt(attackingCharacter.GetWeaponArmorDamageForAction(actionUsed) * extraDmgMult)));
            hitPointsDmg = Mathf.RoundToInt(attackingCharacter.GetWeaponPenetrationDamageForAction(actionUsed) * extraDmgMult);
            
            armorHit = true;
        }
        else
        {
            hitPointsDmg = Mathf.RoundToInt(attackingCharacter.GetWeaponDamageForAction(actionUsed) * extraDmgMult);
        }

        _hitPoints -= Mathf.Max(0, hitPointsDmg);

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

        OnPawnHit.Invoke();
    }

    private void Die()
    {
        _spriteController.Die();
        StartCoroutine(PlayAudioAfterDelay(0f, dieSound));

        _isDead = true;

        UpdateMotivationEvent.Invoke();
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

    /// <summary>
    /// Does the character have enough resources to make any action?
    /// </summary>
    /// <returns></returns>
    public bool HasResourcesForAction(ActionData? theAction = null)
    {
        if (theAction != null)
        {
            if (_actionPoints >= theAction.apCost && _motivation >= theAction.motCost)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            // can still make an attack
            if ((_actionPoints >= GameChar.WeaponItem.baseAction.apCost && _motivation >= GameChar.WeaponItem.baseAction.motCost)
                || (GameChar.WeaponItem.specialAction != null && _actionPoints >= GameChar.WeaponItem.specialAction.apCost && _motivation >= GameChar.WeaponItem.specialAction.motCost))
            {
                return true;
            }
            return false;
        }
    }

    public bool HasActionsRemaining()
    {
        if (HasResourcesForAction())
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
        return _actionPoints >= _gameChar.GetAPPerTileMoved();
    }

    public void HandleActivation()
    {
        UpdateMotivationResource();

        //_audioSource.clip = greedViceSound;
        //_audioSource.Play();
        _spriteController.HandleTurnBegin();
        _actionPoints = BASE_ACTION_POINTS;
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
        if (_actionPoints < _gameChar.GetAPPerTileMoved())
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
            _actionPoints = 0;
        }
        else
        {
            _actionPoints -= _gameChar.GetAPPerTileMoved() * tileDistance;
        }

        _currentTile.PawnExitTile();

        _isMoving = true;
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

        UpdateMotivationEvent.Invoke();

        BattleManager.Instance.PawnActivated(this);

        _isMoving = false;
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

    public int GetInitBuff()
    {
        if (GameChar.Vice == GameCharacter.CharVices.Glory && _isMotivated)
        {
            return 100;
        }
        else
        {
            return 0;
        }
    }

    public void UpdateMotivatedStatus()
    {
        if (IsDead)
        {
            _isMotivated = false;
            return;
        }

        currentEffects.Clear();

        bool wasInMotCondition = _isMotivated;

        // get adjacent enemy pawns at that tile
        List<Pawn> adjEnemyPawns = GetAdjacentEnemies();

        EffectData effectGained = null;

        // if there's no one adjacent, then just lose condition (all conditions
        // are enemy adjacency based)
        if (adjEnemyPawns.Count == 0)
        {
            _isMotivated = false;
        }
        else
        {
            bool in1v1 = false;
            bool isGangedUpOn = false;
            bool isGangingUp = false;

            // only facing one enemy
            if (adjEnemyPawns.Count == 1)
            {
                in1v1 = adjEnemyPawns[0].GetAdjacentEnemies().Count == 1;
            }
            else
            {
                isGangedUpOn = true;
            }

            foreach (Pawn p in adjEnemyPawns)
            {
                if (_currentTile == CurrentTile)
                {
                    if (p.GetAdjacentEnemies().Count > 1)
                    {
                        isGangingUp = true;
                    }
                }
                else
                {
                    if (p.GetAdjacentEnemies().Count > 0)
                    {
                        isGangingUp = true;
                    }
                }
            }

            switch (GameChar.Vice)
            {
                case GameCharacter.CharVices.Honor:
                    _isMotivated = in1v1;
                    effectGained = honorEffect;
                    break;

                case GameCharacter.CharVices.Glory:
                    _isMotivated = isGangedUpOn;
                    effectGained = gloryEffect;
                    break;

                case GameCharacter.CharVices.Greed:
                    _isMotivated = isGangingUp;
                    effectGained = greedEffect;
                    break;

                default:
                    effectGained = honorEffect;
                    Debug.LogWarning("Unhandled vice in effect gained!");
                    break;
            }

            if (_isMotivated && !wasInMotCondition)
            {
                //_audioSource.clip = greedViceSound;
                //_audioSource.Play();

                //_anim.Play("MotivatedGain");
                //_anim.SetBool("IsMotivated", true);

                BattleManager.Instance.AddTextNotification(transform.position, "+ " + effectGained.effectName);
            }
            else if (!_isMotivated && wasInMotCondition)
            {
                //_anim.Play("MotivatedLoss");
                //_anim.SetBool("IsMotivated", false);

                BattleManager.Instance.AddTextNotification(transform.position, "- " + effectGained.effectName);
            }
        }

        if (IsMotivated)
        {
            currentEffects.Add(effectGained);
        }

        OnEffectUpdate.Invoke(currentEffects);
    }

    #endregion
}