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

    [SerializeField] private int _baseAPPerAttack;

    public float baseHitChance;
    public float baseDodgeChance;
    public float baseSurroundBonus;

    public int MoveRange => _actionPoints/_gameChar.GetAPPerTileMoved();

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

    public GameCharacter.CharVices CurrentVice => _gameChar.Vice;

    public int Damage => _gameChar.WeaponItem.damage;

    private void Awake()
    {
        pathfinder.OnDestinationReached.AddListener(HandleDestinationReached);
    }

    public void SetCharacter(GameCharacter character, bool isPlayerTeam)
    {
        _gameChar = character;
        SetTeam(isPlayerTeam);
        _hitPoints = character.HitPoints;
        _armorPoints = character.GetTotalArmor();

        _motivation = GameChar.GetBattleMotivationCap();
        _actionPoints = BASE_ACTION_POINTS;

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
        return Mathf.Max(_actionPoints - (tileDist * _gameChar.GetAPPerTileMoved()), -1);
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
    
    private void UpdateMotivation()
    {
        _motivation = Mathf.Clamp(_motivation + MOT_REGAIN_RATE, _motivation, GameChar.GetBattleMotivationCap());
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

    public void AttackPawnIfResourcesAvailable(Pawn targetPawn)
    {
        if (_actionPoints < _baseAPPerAttack || _motivation < MOT_BASIC_ATTACK_COST)
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
        _motivation -= MOT_BASIC_ATTACK_COST;

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
        if (_actionPoints >= _baseAPPerAttack && _motivation > MOT_BASIC_ATTACK_COST)
        {
            return true;
        }

        // can still move
        if (!EngagedInCombat && _actionPoints >= _gameChar.GetAPPerTileMoved())
        {
            return true;
        }

        return false;
    }

    public void HandleActivation()
    {
        UpdateMotivation();

        _actionPoints = BASE_ACTION_POINTS;
    }

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

        Tile adjustedTargetTile = targetTile;
        int tileDistance = _currentTile.GetTileDistance(targetTile);
        if (_actionPoints < _gameChar.GetAPPerTileMoved() * tileDistance)
        {
            List<Tile> moveOptions = _currentTile.GetTilesInMoveRange();

            for (int i = 0; i < moveOptions.Count; i++)
            {
                Tile t = moveOptions[i];
                if (t.GetPawn() != null)
                {
                    moveOptions.Remove(t);
                }
            }

            moveOptions = moveOptions.OrderBy(x => (x.transform.position - targetTile.transform.position).magnitude).ToList();

            adjustedTargetTile = moveOptions.First();
        }

        Vector3 position = adjustedTargetTile.transform.position;

        pathfinder.destination = position;
        pathfinder.enabled = true;

        _anim.Play("Run");

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
        _currentTile = adjustedTargetTile;
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
    }
}