using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Should manage the flow of the battle (turns), initialization, and reporting results back to GameManager.
/// </summary>
public class FlowDirector : MonoBehaviour
{
    public enum BattleResult
    {
        Win,
        Lose,
        Undecided
    };
    private BattleResult _battleResult;

    public static FlowDirector Instance => _instance;
    private static FlowDirector _instance;

    public List<Pawn> PlayerPawns => _playerPawns;
    private List<Pawn> _playerPawns = new();

    [SerializeField] PhysicalCastle castle;
    [SerializeField] private AIPlayer _aiPlayer;
    [SerializeField] private SelectionManager _selectionManager;
    [SerializeField] private ParticleSystem bloodEffect;
    [SerializeField] private ParticleSystem armorHitEffect;

    [Header("Audio")]
    [SerializeField] private AudioClip _levelUpSound;

    public Pawn CurrentPawn => _currentPawn;
    private Pawn _currentPawn;

    public Stack<Pawn> InitiativeStack => _initiativeStack;
    private Stack<Pawn> _initiativeStack = new ();

    public int TurnNumber => _turnNumber;
    private int _turnNumber = -1;
    private BattleUI _battleUI;
    private PawnSpawner _spawner;

    [SerializeField] private PawnEvents _pawnEvents;

    private DataLoader _dataLoader;

    #region Unity Methods

    private void Awake()
    {
        _instance = this;

        _spawner = GetComponent<PawnSpawner>();
        _battleUI = GetComponent<BattleUI>();
        _battleUI.OnWaveFinished.AddListener(HandleWaveFinished);
        _battleUI.OnWaveBegin.AddListener(StartWave);
        _battleUI.OnEndTurn.AddListener(EndTurn);

        castle.OnGetHit.AddListener(HandleCastleHit);

        _pawnEvents.AddActedListener(HandlePawnActed);
        _pawnEvents.AddKilledListener(HandlePawnKilled);
        _pawnEvents.AddSpawnedListener(HandlePawnSpawned);

        _dataLoader = new DataLoader();
        _dataLoader.LoadData();

        _dataLoader.OnDataLoaded.AddListener(HandleDataLoaded);
    }    

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && CurrentPawn.OnPlayerTeam)
        {
            EndTurn();
        }
    }

    private void OnDestroy()
    {
        _battleUI.OnWaveFinished.RemoveListener(HandleWaveFinished);
        _battleUI.OnWaveBegin.RemoveListener(StartWave);
        _battleUI.OnEndTurn.RemoveListener(EndTurn);

        castle.OnGetHit.RemoveListener(HandleCastleHit);

        _pawnEvents.RemoveActedListener(HandlePawnActed);
        _pawnEvents.RemoveKilledListener(HandlePawnKilled);
        _pawnEvents.RemoveSpawnedListener(HandlePawnSpawned);
        
        _dataLoader.OnDataLoaded.AddListener(HandleDataLoaded);
    }

    #endregion

    #region FX

    public void PlayBloodSpurt(Vector3 location)
    {
        bloodEffect.gameObject.transform.position = location;
        bloodEffect.Play();
    }

    public void PlayArmorHitFX(Vector3 location)
    {
        armorHitEffect.gameObject.transform.position = location;
        armorHitEffect.Play();
    }

    #endregion

    #region Unorganized

    private void HandleDataLoaded()
    {
        _spawner.SpawnPlayerCharacters(); 
        StartWave();
    }

    private void HandleCastleHit(int hpRemaining)
    {
        if (hpRemaining <= 0)
        {
            HandleBattleResult(BattleResult.Lose);
        }
        
        //castleHitPointsUI.text = "Castle HP: " + hpRemaining.ToString();
    }

    private void EndTurn()
    {
        Pawn activePawn = _selectionManager.SelectedTile.GetPawn();
        PawnFinished(activePawn);
    }

    private void HandleWaveFinished()
    {
        // nothing yet!
    }

    /// <summary>
    /// When a pawn has used all AP and it's the next pawn's turn
    /// </summary>
    public void PawnFinished(Pawn p)
    {
        p.HandleTurnEnded();

        if (_selectionManager.SelectedTile != null)
        {
            _selectionManager.SelectedTile.SetSelected(false);
        }

        StartCoroutine(NextPawnCoroutine());
    }

    /// <summary>
    /// When a pawn acts but not necessarily when it's finished
    /// </summary>
    /// <param name="p"></param>
    public void HandlePawnActed(Pawn p)
    {
        StartCoroutine(PawnActedCoroutine(p));
    }

    private IEnumerator PawnActedCoroutine(Pawn p)
    {
        yield return new WaitUntil(() => !p.HoldingForAttackAnimation);

        // level up visuals & audio. Need pauses to allow the player time to
        // process what's going on.
        if (p.PendingLevelUp)
        {

            p.PendingLevelUp = false;

            yield return new WaitForSeconds(.3f);

            p.TriggerLevelUpVisuals();

            yield return new WaitForSeconds(.5f);

            GameObject pooledAudioSourceGO = ObjectPool.Instance.GetAudioSource();
            pooledAudioSourceGO.SetActive(true);
            AudioSource audioSource = pooledAudioSourceGO.GetComponent<AudioSource>();
            audioSource.clip = _levelUpSound;
            audioSource.Play();

            Notifications.Instance.AddPendingTextNotification("Level up!", Color.yellow);
            Notifications.Instance.TriggerTextNotification(p.transform.position);

            yield return new WaitForSeconds(.25f);
        }

        // update selected tile after a potential move
        _selectionManager.SetSelectedTile(CurrentPawn.CurrentTile);

        // see if the battle is over. If so, do sumthin about it 
        if (CheckEnemyWipedOut())
        {
            HandleBattleResult(BattleResult.Win);
        }
        else if (CheckPlayerWipedOut())
        {
            HandleBattleResult(BattleResult.Lose);
        }

        if (!p.HasActionsRemaining())
        {
            PawnFinished(p);
        }
        else if (p.OnPlayerTeam && p.IsPossessed)
        {
            _aiPlayer.DoTurn(p);
        }
        else if (!p.OnPlayerTeam)
        {
            _aiPlayer.DoTurn(p);
        }
    }

    public void HandlePawnKilled(Pawn p)
    {
        if (p.OnPlayerTeam && GameManager.Instance != null)
        {
            GameManager.Instance.RemoveFollower(p.GameChar);
        }
    }

    private void HandlePawnSpawned(Pawn p)
    {
        if (p.GameChar.OnPlayerTeam)
        {
            _playerPawns.Add(p);
        }
        else
        {
            // this needs to be moved somewhere else - probably to pawn - once we figure out the castle stuff
            p.castle = castle;

            _aiPlayer.RegisterPawn(p);
        }
    }

    private List<Pawn> GetFriendlyLivingPawns()
    {
        List<Pawn> livingPawns = new();
        foreach (Pawn p in _playerPawns)
        {
            if (!p.IsDead)
            {
                livingPawns.Add(p);
            }
        }

        return livingPawns;
    }

    private void NewTurn()
    {
        _turnNumber++;

        _spawner.SpawnEnemiesForTurn();

        RefreshInitiativeStack();

        _battleUI.SetTurnUI(_turnNumber);
        _battleUI.RefreshInitStackUI();
    }

    private void RefreshInitiativeStack()
    {
        List<Pawn> pawnList = new();

        foreach (Pawn p in GetFriendlyLivingPawns()) { pawnList.Add(p); }
        foreach (Pawn p in _aiPlayer.GetEnemyLivingPawns()) { pawnList.Add(p); }
        
        pawnList = pawnList.OrderBy(pawn => pawn.Initiative).ToList();

        // this way the stack can be sorted properly 
        _initiativeStack = new(pawnList);
    }

    private void StartWave()
    {
        _spawner.PrepareNewWave();
        _battleResult = BattleResult.Undecided;
        _turnNumber = 0;
        StartCoroutine(NextPawnCoroutine());
    }

    private void HandleBattleResult(BattleResult battleResult)
    {
        // distribute XP for participating in battle
        for (int i = 0; i < _playerPawns.Count; i++)
        {
            _playerPawns[i].HandleBattleEnd();
        }

        _battleUI.HandleBattleResult(battleResult);

        _battleResult = battleResult;
    }

    private bool CheckEnemyWipedOut()
    {
        return _aiPlayer.GetEnemyLivingPawns().Count <= 0;
    }

    private bool CheckPlayerWipedOut()
    {
        int alivePawns = 0;
        foreach (Pawn p in _playerPawns)
        {
            if (p.IsDead)
            {
                continue;
            }
            alivePawns++;
        }

        return alivePawns <= 0;
    }

    public IEnumerator NextPawnCoroutine()
    {
        // pause a little bit so the player can keep track of what the heck is happening
        // was using await here to avoid coroutine, but web builds can't use await.
        yield return new WaitForSeconds(.25f);

        if (_currentPawn != null)
        {
            _currentPawn.OnEffectUpdate.RemoveListener(_battleUI.UpdateEffects);
        }

        _currentPawn = GetNextPawn();

        yield return new WaitUntil(() => !_currentPawn.HoldingForAttackAnimation);

        // AddTextNotification(_currentPawn.transform.position, new () {(_currentPawn.OnPlayerTeam ? "For God and Glory!" : "FOR THE DARK GODS!", Color.white)});

        // see if the battle is over. If so, do sumthin about it 
        if (CheckEnemyWipedOut())
        {
            HandleBattleResult(BattleResult.Win);
        }
        else
        {
            if (CheckPlayerWipedOut())
            {
                HandleBattleResult(BattleResult.Lose);
            }
            else
            {
                _currentPawn.HandleTurnBegin();
                _battleUI.UpdateUIForPawn(_currentPawn);
                _currentPawn.OnEffectUpdate.AddListener(_battleUI.UpdateEffects);

                _selectionManager.HandleTurnChange(_currentPawn.OnPlayerTeam);
                
                if (!_currentPawn.OnPlayerTeam)
                {
                    _selectionManager.DisablePlayerControls();
                    _aiPlayer.DoTurn(_currentPawn);
                }        
                else if (_currentPawn.OnPlayerTeam && _currentPawn.GameChar.RollPosessed())
                {
                    _selectionManager.DisablePlayerControls();
                    _currentPawn.IsPossessed = true;
                    Notifications.Instance.AddPendingTextNotification("Possession!", Color.yellow);
                    Notifications.Instance.TriggerTextNotification(_currentPawn.transform.position);
                    _aiPlayer.DoTurn(_currentPawn);
                }
                else
                {
                    _selectionManager.EnablePlayerControls();
                }
            }
        }
    }

    /// <summary>
    /// Get next pawn in init stack, if no living pawns left return null
    /// </summary>
    /// <returns></returns>
    private Pawn GetNextPawn()
    {
        if (_initiativeStack.Count == 0)
        {
            NewTurn();
        }

        Pawn p = _initiativeStack.Pop();

        while (p.IsDead)
        {
            if (_initiativeStack.Count != 0)
            {
                p = _initiativeStack.Pop();
            }
            else
            {
                NewTurn();
            }
        }

        return p;
    }

    #endregion
}
