using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Pawn : MonoBehaviour
{
    public int MoveRange => moveRange;
    private int moveRange = 4;

    public Tile CurrentTile => _currentTile;
    private Tile _currentTile;

    public bool OnPlayerTeam => _onPlayerTeam;
    private bool _onPlayerTeam;

    public int HitPoints => _hitPoints;
    [SerializeField] private int _hitPoints;

    public bool IsDead => _isDead;
    private bool _isDead;

    [SerializeField] AIPathCustom pathfinder;
    [SerializeField] private Animator _anim;
    [SerializeField] private Sprite _enemyHeadSprite;
    [SerializeField] private Sprite _enemyBodySprite;
    [SerializeField] private Sprite _enemyLegSprite;
    [SerializeField] private SpriteRenderer _headSpriteRend;
    [SerializeField] private SpriteRenderer _bodySpriteRend;
    [SerializeField] private SpriteRenderer _leg1SpriteRend;
    [SerializeField] private SpriteRenderer _leg2SpriteRend;

    private void Awake()
    {
        pathfinder.OnDestinationReached.AddListener(HandleDestinationReached);
    }

    public void SetTeam(bool onPlayerTeam)
    {
        _onPlayerTeam = onPlayerTeam;
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

    public void AttackPawn(Pawn targetPawn)
    {
        _anim.Play("Attack");
        targetPawn.TakeDamage();

        BattleManager.Instance.PawnActivated();
    }

    public void TakeDamage()
    {
        _anim.Play("GetHit");
        _hitPoints--;

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
    }

    public void MoveToTile(Tile actionTile)
    {
        Vector3 position = actionTile.transform.position;

        pathfinder.destination = position;
        pathfinder.enabled = true;

        _anim.Play("Run");

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

        BattleManager.Instance.PawnActivated();
    }

    private void OnDestroy()
    {
        pathfinder.OnDestinationReached.RemoveListener(HandleDestinationReached);
    }
}