using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Pawn : MonoBehaviour
{
    public int MoveRange => moveRange;
    private int moveRange = 4;

    public bool HasMovedThisTurn => _hasMovedThisTurn;
    private bool _hasMovedThisTurn;

    public Tile CurrentTile => _currentTile;
    private Tile _currentTile;

    public bool OnPlayerTeam => _onPlayerTeam;

    [SerializeField] private bool _onPlayerTeam;
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
    }

    private void PickStartTile()
    {
        Tile spawnTile;
        if (OnPlayerTeam)
        {
            spawnTile = GridGenerator.Instance.PlayerSpawns[Random.Range(0, GridGenerator.Instance.PlayerSpawns.Count)];
        }
        else
        {
            spawnTile = GridGenerator.Instance.EnemySpawns[Random.Range(0, GridGenerator.Instance.EnemySpawns.Count)];
        }

        _currentTile = spawnTile;
        spawnTile.PawnEnterTile(this);
        transform.position = spawnTile.transform.position;
    }

    public void ActOnTile(Tile actionTile)
    {
        Vector3 position = actionTile.transform.position;

        pathfinder.destination = position;
        pathfinder.enabled = true;

        _anim.Play("Run");

        _hasMovedThisTurn = true;
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

        BattleManager.Instance.PawnMoved(this);
    }

    private void OnDestroy()
    {
        pathfinder.OnDestinationReached.RemoveListener(HandleDestinationReached);
    }
}