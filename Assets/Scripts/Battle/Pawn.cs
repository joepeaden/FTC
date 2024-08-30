using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Linq;
using UnityEngine.Events;

public class Pawn : MonoBehaviour
{
    /// <summary>
    /// Lol idk what to call it. Update motivation for pawns.
    /// </summary>
    private static UnityEvent PleaseUpdateMotivation = new();

    private const int BASE_MOTIVATION = 3;
    private const int MIN_MOTIVATION = 1;
    private const int MAX_MOTIVATION = 6;

    [SerializeField] private int _baseAPPerTileMoved;
    [SerializeField] private int _baseAPPerAttack;

    public float baseHitChance;
    public float baseDodgeChance;
    public float baseSurroundBonus;

    public int MoveRange => _actionPoints/_baseAPPerTileMoved;

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

    public int MaxActionPoints => Mathf.Clamp(_motivation * 2, 3, MAX_MOTIVATION*2);
    public int ActionPoints => _actionPoints;
    private int _actionPoints;

    public int MaxMotivation => MAX_MOTIVATION;
    public int Motivation => _motivation;
    private int _motivation;

    public bool IsDead => _isDead;
    private bool _isDead;

    public bool EngagedInCombat => GetAdjacentEnemies().Count > 0;

    [SerializeField] AIPathCustom pathfinder;
    [SerializeField] private Animator _anim;
    [SerializeField] private Sprite _enemyHeadSprite;
    [SerializeField] private Sprite _enemyBodySprite;
    [SerializeField] private Sprite _enemyLegSprite;
    [SerializeField] private SpriteRenderer _headSpriteRend;
    [SerializeField] private SpriteRenderer _bodySpriteRend;
    [SerializeField] private SpriteRenderer _leg1SpriteRend;
    [SerializeField] private SpriteRenderer _leg2SpriteRend;
    [SerializeField] private SpriteRenderer _helmSpriteRend;
    [SerializeField] private SpriteRenderer _weaponSpriteRend;

    public GameCharacter GameChar => _gameChar;
    private GameCharacter _gameChar;

    public GameCharacter.Motivator CurrentMotivator => _currentMotivator;
    private GameCharacter.Motivator _currentMotivator;

    public int Damage => _gameChar.WeaponItem.damage;

    private void Awake()
    {
        pathfinder.OnDestinationReached.AddListener(HandleDestinationReached);
        PleaseUpdateMotivation.AddListener(UpdateMotivation);
    }

    public void SetCharacter(GameCharacter character, bool isPlayerTeam)
    {
        _gameChar = character;
        SetTeam(isPlayerTeam);
        _currentMotivator = character.GetBiggestMotivator();
        _hitPoints = character.HitPoints;
        _armorPoints = character.GetTotalArmor();

        // eventually put this in GameCharacter, for now just affected by battle
        _motivation = BASE_MOTIVATION;
        _actionPoints = MaxActionPoints;

        if (_gameChar.WeaponItem != null)
        {
            _weaponSpriteRend.sprite = _gameChar.WeaponItem.itemSprite;
        }

        if (_gameChar.HelmItem != null)
        {
            _helmSpriteRend.sprite = _gameChar.HelmItem.itemSprite;
        }
    }

    public void SetTeam(bool onPlayerTeam)
    {
        _onPlayerTeam = onPlayerTeam;
    }

    public int GetAPAfterAttack()
    {
        return ActionPoints - _baseAPPerAttack;
    }

    public int GetAPAfterMove(Tile targetTile)
    {
        // will have no AP if leaving combat
        if (EngagedInCombat)
        {
            return 0;
        }

        int tileDist = _currentTile.GetTileDistance(targetTile);
        return Mathf.Max(_actionPoints - (tileDist * _baseAPPerTileMoved), -1);
    }

    public Sprite GetFaceSprite()
    {
        // later, can return more than just a single sprite. For example wounds, current equipment, headgear,
        // hair, etc. I'm not sure how that will work.

        //if (GameChar.FaceSprite == null)
        //{
            return _headSpriteRend.sprite;
        //}
    }

    public Sprite GetHelmSprite()
    {
        return _helmSpriteRend.sprite;
    }

    private void Start()
    {
        PickStartTile();
        _anim.Play("Idle");

        if (!OnPlayerTeam)
        {
            _headSpriteRend.sprite = _enemyHeadSprite;
            _bodySpriteRend.sprite = _enemyBodySprite;
            _leg1SpriteRend.sprite = _enemyLegSprite;
            _leg2SpriteRend.sprite = _enemyLegSprite;
        }
        else
        {
            transform.rotation *= Quaternion.Euler(0, 180, 0);
        }
    }

    private bool DecayMotivationIfNeeded()
    {
        List<Pawn> adjPawns = GetAdjacentEnemies();

        // if there's no one adjacent, then no positive motivation condition (for now),
        // decay motivation a little if above threshold.
        if (adjPawns.Count == 0)
        {
            if (_motivation > BASE_MOTIVATION)
            {
                _motivation--;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Call to update motivation based on current situation
    /// </summary>
    /// <param name="motivator"></param>
    private void UpdateMotivation()
    {
        int oldMotivation = _motivation;

        bool motDecayed = DecayMotivationIfNeeded();
        if (!motDecayed)
        {
            _motivation = GetMotivationAtTile(_currentTile);
        }

        string uiString = (_motivation > oldMotivation ? "+" : "-") + " MOT";

        if (oldMotivation != _motivation)
        {
            BattleManager.Instance.AddTextNotification(transform.position, uiString);
        }
    }

    public int GetMotivationAtTile(Tile targetTile)
    {
        // get adjacent enemy pawns at that tile
        List<Pawn> adjEnemyPawns = targetTile.GetAdjacentTiles()
            .Where(tile => tile.GetPawn() != null && tile.GetPawn().OnPlayerTeam != _onPlayerTeam)
            .Select(tile => tile.GetPawn())
            .ToList();

        // if there's no one adjacent, then no conditions
        if (adjEnemyPawns.Count == 0)
        {
            return _motivation;
        }

        int newMotivation = _motivation;

        bool in1v1 = false;
        bool isGangedUpOn = false;
        bool isGangingUp = false;

        // only facing one enemy
        if (adjEnemyPawns.Count == 1)
        {
            if (_currentTile == targetTile)
            {
                in1v1 = adjEnemyPawns[0].GetAdjacentEnemies().Count == 1;
            }
            else
            {
                // 0 because this pawn isn't there yet (if we're previewing)
                in1v1 = adjEnemyPawns[0].GetAdjacentEnemies().Count == 0;
            }
        }
        else
        {
            isGangedUpOn = true;
        }

        foreach (Pawn p in adjEnemyPawns)
        {
            if (_currentTile == targetTile)
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

        switch (_currentMotivator)
        {
            case GameCharacter.Motivator.Sanctimony:
                
                if (in1v1)
                {
                    newMotivation += 1;
                }
                
                if (isGangingUp)
                {
                    newMotivation -= 1;
                }

                break;

            case GameCharacter.Motivator.Vainglory:
                if (isGangedUpOn)
                {
                    newMotivation += 1;
                }

                if (in1v1)
                {
                    newMotivation -= 1;
                }
                break;

            case GameCharacter.Motivator.Avarice:
                if (isGangingUp)
                {
                    newMotivation += 1;
                }

                if (isGangedUpOn)
                {
                    newMotivation -= 1;
                }
                break;

        }

        // make sure to stay in the valid range of motivation
        return Mathf.Clamp(newMotivation, MIN_MOTIVATION, MAX_MOTIVATION);
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
            } while (spawnTile.GetPawn() != null) ;
        }

        _currentTile = spawnTile;
        spawnTile.PawnEnterTile(this);
        transform.position = spawnTile.transform.position;
    }

    public List<Pawn> GetAdjacentEnemies()
    {
        List<Pawn> adjacentEnemies = new();
        foreach (Tile t in _currentTile.GetAdjacentTiles())
        {
            Pawn p = t.GetPawn();
            if (p != null && p.OnPlayerTeam != _onPlayerTeam)
            {
                adjacentEnemies.Add(p);
            }
        }

        return adjacentEnemies;
    }

    public float GetHitChance(Pawn targetPawn)
    {
        float motivationHitBonus = 0f;
        switch (_motivation)
        {
            case 1:
                motivationHitBonus = -0.1f;
                break;
            case 2:
                motivationHitBonus = -0.05f;
                break;
            case 3:
                motivationHitBonus = 0f;
                break;
            case 4:
                motivationHitBonus = 0.05f;
                break;
            case 5:
                motivationHitBonus = 0.1f;
                break;
            case 6:
                motivationHitBonus = 0.15f;
                break;
        }

        float hitChance = baseHitChance + motivationHitBonus;
        return hitChance;
    }

    public void AttackPawnIfAPAvailable(Pawn targetPawn)
    {
        if (_actionPoints < _baseAPPerAttack)
        {
            return;
        }

        _anim.Play("Attack");

        float hitChance = GetHitChance(targetPawn);
        float hitRoll = Random.Range(0f, 1f);
        if (hitRoll < hitChance)
        {
            targetPawn.TakeDamage(_gameChar.WeaponItem.damage);
            BattleManager.Instance.AddTextNotification(transform.position, "Hit!");
        }
        else
        {
            BattleManager.Instance.AddTextNotification(transform.position, "Miss!");
        }

        _actionPoints -= _baseAPPerAttack;

        BattleManager.Instance.PawnActivated(this);
    }

    public void TakeDamage(int incomingDamage)
    {
        _anim.Play("GetHit");

        if (_armorPoints > 0)
        {
            _armorPoints = Mathf.Max(0, (_armorPoints - incomingDamage));
        }
        else
        {
            _hitPoints -= incomingDamage;
        }

        if (_hitPoints <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        _anim.Play("Die");
        CurrentTile.PawnExitTile();
        _isDead = true;

        BattleManager.Instance.PawnKilled(this);
    }

    public bool HasActionsRemaining()
    {
        // can still make an attack
        if (_actionPoints >= _baseAPPerAttack)
        {
            return true;
        }

        // can still move
        if (!EngagedInCombat && _actionPoints >= _baseAPPerTileMoved)
        {
            return true;
        }

        return false;
    }

    public void HandleActivation()
    {

        UpdateMotivation();
        //PleaseUpdateMotivation.Invoke();

        _actionPoints = MaxActionPoints;
    }

    public void MoveToTileIfAPAvailable(Tile actionTile)
    {
        int tileDistance = _currentTile.GetTileDistance(actionTile);
        if (_actionPoints < _baseAPPerTileMoved * tileDistance)
        {
            return;
        }

        Vector3 position = actionTile.transform.position;

        pathfinder.destination = position;
        pathfinder.enabled = true;

        _anim.Play("Run");

        // use whole turn to get out of combat with someone to avoid player
        // going in and out of combat repeatedly to gain motivation
        if (EngagedInCombat)
        {
            _actionPoints = 0;
        }
        else
        {
            _actionPoints -= _baseAPPerTileMoved * tileDistance;
        }

        _currentTile.PawnExitTile();
        _currentTile = actionTile;
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
                pathfinder.enabled = false;
                break;
            }
        }

        _anim.Play("Idle");

        BattleManager.Instance.PawnActivated(this);
    }

    private void OnDestroy()
    {
        pathfinder.OnDestinationReached.RemoveListener(HandleDestinationReached);
        PleaseUpdateMotivation.RemoveListener(UpdateMotivation);
    }
}