using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    private const int DEFAULT_MIN_AMOUNT_TO_SPAWN = 3;

    private const int DEFAULT_MAX_AMOUNT_TO_SPAWN = 8;

    enum BattleResult
    {
        Win,
        Lose,
        Undecided
    };
    private BattleResult _battleResult;

    public static BattleManager Instance => _instance;
    private static BattleManager _instance;

    public List<Pawn> PlayerPawns => _playerPawns;
    private List<Pawn> _playerPawns = new();

    [SerializeField] private Button _endTurnButton;
    [SerializeField] TMP_Text turnText;
    [SerializeField] GameObject turnUI;
    [SerializeField] TMP_Text winLoseText;
    [SerializeField] GameObject winLoseUI;
    [SerializeField] private EnemyAI _enemyAI;
    [SerializeField] private Button gameOverButton;
    [SerializeField] private SelectionManager _selectionManager;
    [SerializeField] GameObject pawnPrefab;
    [SerializeField] Transform enemyParent;
    [SerializeField] Transform friendlyParent;
    [SerializeField] TMP_Text hitChanceText;
    [SerializeField] TMP_Text characterNameText;
    [SerializeField] TMP_Text characterMotivatorText;
    [SerializeField] PawnHeadPreview currentPawnPreview;
    [SerializeField] TMP_Text armorText;
    [SerializeField] StatBar armorBar;
    [SerializeField] TMP_Text healthText;
    [SerializeField] StatBar healthBar;
    [SerializeField] TMP_Text apText;
    [SerializeField] StatBar apBar;
    [SerializeField] TMP_Text motText;
    [SerializeField] StatBar motBar;
    [SerializeField] private Transform _initStackParent;
    [SerializeField] GameObject pawnPreviewPrefab;
    [SerializeField] private List<TextFloatUp> _floatingTexts = new();
    [SerializeField] private CharacterTooltip charTooltip;
    [SerializeField] private GameObject _instructionsUI;
    [SerializeField] private Button _startBattleButton;
    [SerializeField] private Transform _actionsParent;
    [SerializeField] private GameObject _actionButtonPrefab;
    [SerializeField] private GameObject bottomUIObjects;
    [SerializeField] private ParticleSystem bloodEffect;
    [SerializeField] private ParticleSystem armorHitEffect;

    [Header("UI")]
    [SerializeField] private Transform _healthBarParent;
    [SerializeField] private MiniStatBar _miniStatBarPrefab;

    [Header("Equipment")]
    [SerializeField] private ArmorItemData lightHelm;
    [SerializeField] private ArmorItemData medHelm;
    [SerializeField] private ArmorItemData heavyHelm;
    [SerializeField] private WeaponItemData club;
    [SerializeField] private WeaponItemData sword;
    [SerializeField] private WeaponItemData axe;
    [SerializeField] private WeaponItemData spear;

    public Pawn CurrentPawn => _currentPawn;
    private Pawn _currentPawn;

    private List<ActionButton> actionButtons = new();

    public ActionData CurrentAction => _currentAction;
    private ActionData _currentAction;

    private Stack<Pawn> _initiativeStack = new ();

    public int TurnNumber => _turnNumber;
    private int _turnNumber = -1;

    private List<Tile> tilesToHighlight = new();

    private void Awake()
    {
        _instance = this;
        gameOverButton.onClick.AddListener(ExitBattle);

        int dudesToSpawn;
        if (GameManager.Instance != null)
        {
            dudesToSpawn = GameManager.Instance.GetNumOfEnemiesToSpawn();

            Pawn playerPawn = Instantiate(pawnPrefab, friendlyParent).GetComponent<Pawn>();
            _playerPawns.Add(playerPawn);
            playerPawn.SetCharacter(GameManager.Instance.PlayerCharacter, true);

            foreach (GameCharacter character in GameManager.Instance.PlayerFollowers)
            {
                Pawn newPawn = Instantiate(pawnPrefab, friendlyParent).GetComponent<Pawn>();
                _playerPawns.Add(newPawn);
                newPawn.SetCharacter(character, true);

                MiniStatBar miniStats = Instantiate(_miniStatBarPrefab, _healthBarParent);
                miniStats.SetData(newPawn);
            }
        }
        else
        {
            // change game over button text to "restart"
            gameOverButton.GetComponentInChildren<TMP_Text>().text = "Restart";

            Debug.Log("No game manager, spawning default amount");
            dudesToSpawn = Random.Range(DEFAULT_MIN_AMOUNT_TO_SPAWN, DEFAULT_MAX_AMOUNT_TO_SPAWN);

            // spawn some friendlies
            for (int i = 0; i < dudesToSpawn; i++)
            {
                Pawn newPawn = Instantiate(pawnPrefab, friendlyParent).GetComponent<Pawn>();
                _playerPawns.Add(newPawn);

                GameCharacter guy = new GameCharacter();

                if (GameManager.Instance == null)
                {
                    // pick random weapon
                    int roll = Random.Range(0, 4);
                    switch (roll)
                    {
                        case 0:
                            guy.EquipItem(club);
                            break;
                        case 1:
                            guy.EquipItem(sword);
                            break;
                        case 2:
                            guy.EquipItem(spear);
                            break;
                        case 3:
                            guy.EquipItem(axe);
                            break;
                    }

                    // pick random armor
                    roll = Random.Range(0, 4);
                    switch (roll)
                    {
                        case 0:
                            // no armor
                            break;
                        case 1:
                            guy.EquipItem(lightHelm);
                            break;
                        case 2:
                            guy.EquipItem(heavyHelm);
                            break;
                        case 3:
                            guy.EquipItem(medHelm);
                            break;
                    }

                }
                else
                {
                    guy.EquipItem(club);
                }

                newPawn.SetCharacter(guy, true);

                MiniStatBar miniStats = Instantiate(_miniStatBarPrefab, _healthBarParent);
                miniStats.SetData(newPawn);
            }
        }

        dudesToSpawn = Random.Range(DEFAULT_MIN_AMOUNT_TO_SPAWN, DEFAULT_MAX_AMOUNT_TO_SPAWN);
        for (int i = 0; i < dudesToSpawn; i++)
        {
            Pawn newPawn = Instantiate(pawnPrefab, enemyParent).GetComponent<Pawn>();

            GameCharacter guy = new();

            if (GameManager.Instance == null)
            {
                // pick random weapon
                int roll = Random.Range(0, 4);
                switch(roll)
                {
                    case 0:
                        guy.EquipItem(club);
                        break;
                    case 1:
                        guy.EquipItem(sword);
                        break;
                    case 2:
                        guy.EquipItem(spear);
                        break;
                    case 3:
                        guy.EquipItem(axe);
                        break;
                }

                // pick random armor
                roll = Random.Range(0, 4);
                switch (roll)
                {
                    case 0:
                        // no armor
                        break;
                    case 1:
                        guy.EquipItem(lightHelm);
                        break;
                    case 2:
                        guy.EquipItem(heavyHelm);
                        break;
                    case 3:
                        guy.EquipItem(medHelm);
                        break;
                }

            }
            else
            {
                guy.EquipItem(club);
            }

            newPawn.SetCharacter(guy, false);

            _enemyAI.RegisterPawn(newPawn);

            MiniStatBar miniStats = Instantiate(_miniStatBarPrefab, _healthBarParent);
            miniStats.SetData(newPawn);
        }

        Tile.OnTileHoverStart.AddListener(HandleTileHoverStart);
        Tile.OnTileHoverEnd.AddListener(HandleTileHoverEnd);
        _endTurnButton.onClick.AddListener(EndTurn);

        _startBattleButton.onClick.AddListener(ToggleInstructions);
    }

    private void Start()
    {
        _battleResult = BattleResult.Undecided;
    }

    private void OnDestroy()
    {
        gameOverButton.onClick.RemoveListener(ExitBattle);

        Tile.OnTileHoverStart.RemoveListener(HandleTileHoverStart);
        Tile.OnTileHoverEnd.RemoveListener(HandleTileHoverEnd);
        _endTurnButton.onClick.RemoveListener(EndTurn);
        _startBattleButton.onClick.RemoveListener(ToggleInstructions);
    }

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

    public void AddTextNotification(Vector3 pos, string str)
    {
        foreach (TextFloatUp txt in _floatingTexts)
        {
            if (txt.InUse)
            {
                continue;
            }
            else
            {
                txt.SetData(pos, str, Color.white);
                break;
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleInstructions();
        }

        if (Input.GetMouseButtonDown(1))
        {
            ClearSelectedAction();
        }

    }

    private void ToggleInstructions()
    {
        _instructionsUI.SetActive(!_instructionsUI.activeInHierarchy);
        if (_turnNumber == -1)
        {
            StartBattle();
        }
    }

    private void UpdateUIForPawn(Pawn p)
    {
        if (p != null)
        {
            bottomUIObjects.SetActive(true);
            characterNameText.text = p.GameChar.CharName;
            characterMotivatorText.text = p.CurrentVice.ToString();
            armorText.text = "AR: " + p.ArmorPoints + "/" + p.MaxArmorPoints;
            armorBar.SetBar(p.MaxArmorPoints, p.ArmorPoints);
            healthText.text = "HP: " + p.HitPoints + "/" + p.MaxHitPoints;
            healthBar.SetBar(p.MaxHitPoints, p.HitPoints);
            apText.text = "AP: " + p.ActionPoints + "/" + Pawn.BASE_ACTION_POINTS;
            apBar.SetBar(Pawn.BASE_ACTION_POINTS, p.ActionPoints);
            motText.text = "MT: " + p.Motivation + "/" + p.MaxMotivation;
            motBar.SetBar(p.MaxMotivation, p.Motivation);

            currentPawnPreview.SetData(p);
            //currentPawnFace.sprite = p.GetFaceSprite();
            //currentPawnHelm.sprite = p.GetFaceSprite();

            RefreshInitStackUI();

            for (int i = 0; i < _actionsParent.childCount; i++)
            {
                Destroy(_actionsParent.GetChild(i).gameObject);
            }

            actionButtons.Clear();

            if (p.OnPlayerTeam)
            {
                GameObject actionButtonGO = Instantiate(_actionButtonPrefab, _actionsParent);
                actionButtonGO.GetComponent<ActionButton>().SetDataButton(p.GameChar.WeaponItem.baseAction, HandleActionClicked, KeyCode.Alpha1);
                actionButtons.Add(actionButtonGO.GetComponent<ActionButton>());
                if (p.GameChar.WeaponItem.specialAction != null)
                {
                    actionButtonGO = Instantiate(_actionButtonPrefab, _actionsParent);
                    actionButtonGO.GetComponent<ActionButton>().SetDataButton(p.GameChar.WeaponItem.specialAction, HandleActionClicked, KeyCode.Alpha2);
                    actionButtons.Add(actionButtonGO.GetComponent<ActionButton>());
                }
            }
        }
    }

    public void ClearSelectedAction()
    {
        foreach (ActionButton abutton in actionButtons)
        {
            abutton.SetInactive();
        }

        if (_currentAction != null)
        {
            CurrentPawn.CurrentTile.HighlightTilesInRange(_currentAction.range + 1, false, Tile.TileHighlightType.AttackRange);
            _currentAction = null;

            if (CurrentPawn.HasActionsRemaining())
            {
                CurrentPawn.CurrentTile.HighlightTilesInRange(_currentPawn.MoveRange+1, true, Tile.TileHighlightType.Move);
            }
        }
    }

    private void HandleActionClicked(ActionData action)
    {
        ClearSelectedAction();

        _currentAction = action;

        CurrentPawn.CurrentTile.HighlightTilesInRange(_currentPawn.MoveRange+1, false, Tile.TileHighlightType.Move);
        CurrentPawn.CurrentTile.HighlightTilesInRange(_currentAction.range+1, true, Tile.TileHighlightType.AttackRange);
    }

    private void EndTurn()
    {
        Pawn activePawn = _selectionManager.SelectedTile.GetPawn();
        PawnFinished(activePawn);
    }

    public void HandleTileHoverStart(Tile targetTile)
    {
        if (_selectionManager.SelectedTile == null)
        {
            return;
        }

        Pawn selectedPawn = _selectionManager.SelectedTile.GetPawn();
        Pawn targetPawn = targetTile.GetPawn();

        if (selectedPawn != null)
        {
            if (_currentAction != null && targetTile.GetTileDistance(selectedPawn.CurrentTile) <= _currentAction.range)// ;// && targetPawn.OnPlayerTeam != selectedPawn.OnPlayerTeam && targetPawn.IsTargetInRange(selectedPawn, _currentAction))
            {
                tilesToHighlight.Clear();
                tilesToHighlight.Add(targetTile);
                if (_currentAction != null)
                {
                    if (_currentAction.attackStyle == ActionData.AttackStyle.LShape)
                    {
                        tilesToHighlight.Add(selectedPawn.CurrentTile.GetClockwiseNextTile(targetTile));
                    }
                }

                foreach (Tile t in tilesToHighlight)
                {
                    t.HighlightForAction();
                }

                apBar.SetBar(Pawn.BASE_ACTION_POINTS, selectedPawn.ActionPoints, selectedPawn.GetAPAfterAction(_currentAction));
            }

            // if targetPawn is null, then we're hovering a movement tile.
            if (targetPawn == null)
            {
                    int expectedAPAfterMove = selectedPawn.GetAPAfterMove(targetTile);
                    apBar.SetBar(Pawn.BASE_ACTION_POINTS, selectedPawn.ActionPoints, expectedAPAfterMove);
                    // otherwise we might be hovering ourselves or a teammate so reset the AP Bar
                    //apBar.SetBar(Pawn.BASE_ACTION_POINTS, selectedPawn.ActionPoints);
            }
            else
            {
                ShowTooltipForPawn(targetPawn);
            }
        }
    }

    public void HandleTileHoverEnd(Tile t)
    {
        HideHitChance();

        foreach(Tile highlightedTile in tilesToHighlight)
        {
            highlightedTile.ClearActionHighlight();
        }
        tilesToHighlight.Clear();
    }

    public void HideHitChance()
    {
        hitChanceText.gameObject.SetActive(false);
        charTooltip.Hide();
    }

    public void ShowTooltipForPawn(Pawn p)
    {
        charTooltip.SetPawn(p);

        Pawn selectedPawn = _selectionManager.SelectedTile.GetPawn();
        if (selectedPawn.GetAdjacentEnemies().Contains(p))
        {
            float chance = selectedPawn.GetHitChance(p);
            charTooltip.ShowHitPreview(chance);//, selectedPawn.Damage);
        }
    }


    private void ExitBattle()
    {
        if (GameManager.Instance != null)
        {
            bool playerWon = _battleResult == BattleResult.Win;
            GameManager.Instance.ExitBattle(playerWon);
        }
        else
        {
            // easy reload for testing
            SceneManager.LoadScene("BattleScene");
        }
    }

    /// <summary>
    /// When a pawn has used all AP and it's the next pawn's turn
    /// </summary>
    public void PawnFinished(Pawn p)
    {
        if (_selectionManager.SelectedTile != null)
        {
            _selectionManager.SelectedTile.SetSelected(false);
        }

        StartCoroutine(NextActivation());
    }

    /// <summary>
    /// When a pawn acts but not necessarily when it's finished
    /// </summary>
    /// <param name="p"></param>
    public void PawnActivated(Pawn p)
    {
        UpdateUIForPawn(p);
        _selectionManager.SetSelectedTile(p.CurrentTile);

        ClearSelectedAction();

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
        else if (!p.OnPlayerTeam)
        {
            _enemyAI.DoTurn(p);
        }
    }

    public void PawnKilled(Pawn p)
    {
        if (p.OnPlayerTeam && GameManager.Instance != null)
        {
            GameManager.Instance.RemoveFollower(p.GameChar);
        }

        RefreshInitStackUI();
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

    private void RefreshInitiativeStack()
    {
        _turnNumber++;
        turnText.text = _turnNumber.ToString();

        List<Pawn> pawnList = new();

        foreach (Pawn p in GetFriendlyLivingPawns())
        {
            pawnList.Add(p);
        }

        foreach (Pawn p in _enemyAI.GetLivingPawns())
        {
            pawnList.Add(p);
        }
        
        pawnList = pawnList.OrderBy(pawn => pawn.GameChar.GetInitiative()).ToList();

        // this way the stack can be sorted properly 
        _initiativeStack = new(pawnList);
    }

    private void StartBattle()
    {
        _turnNumber = 0;
        _instructionsUI.SetActive(false);
        StartCoroutine(NextActivation());
    }

    public void RefreshInitStackUI()
    {
        for (int i = 0; i < _initStackParent.childCount; i++)
        {
            Transform child = _initStackParent.GetChild(i);
            Destroy(child.gameObject);
        }

        int newChildCount = 0;
        foreach (Pawn p in _initiativeStack)
        {
            // the UI only has space for 7 to look pretty
            if (newChildCount > 7)
                break;

            if (p.IsDead)
            {
                continue;
            }

            GameObject pawnPreview = Instantiate(pawnPreviewPrefab, _initStackParent);
            pawnPreview.GetComponent<PawnHeadPreview>().SetData(p);
            newChildCount++;
        }
    }

    private void HandleBattleResult(BattleResult battleResult)
    {
        turnUI.SetActive(false);
        winLoseUI.SetActive(true);
        bottomUIObjects.SetActive(false);
        winLoseText.text = battleResult == BattleResult.Win ? "Victory!" : "Defeat!" ;
        _battleResult = battleResult;
    }

    private bool CheckEnemyWipedOut()
    {
        return _enemyAI.GetLivingPawns().Count <= 0;
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

    public IEnumerator NextActivation()
    {
        _currentPawn = GetNextPawn();

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
                if (_currentPawn.CurrentTile == null)
                {
                    yield return new WaitUntil(() => _currentPawn.CurrentTile != null);
                }

                _currentPawn.HandleActivation();
                UpdateUIForPawn(_currentPawn);
                _selectionManager.HandleTurnChange(_currentPawn.OnPlayerTeam);
                _endTurnButton.gameObject.SetActive(_currentPawn.OnPlayerTeam);

                if (_currentPawn.OnPlayerTeam)
                {
                    _selectionManager.SetSelectedTile(_currentPawn.CurrentTile);
                }
                else
                {
                    _enemyAI.DoTurn(_currentPawn);
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
            RefreshInitiativeStack();
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
                RefreshInitiativeStack();
            }
        }

        return p;
    }

}
