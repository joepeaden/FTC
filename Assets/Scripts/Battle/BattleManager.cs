using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Manages the Battle scene.
/// Quite clearly, this class should be broken up into 2 - 3 scripts (UI at least should be isolated)
/// </summary>
public class BattleManager : MonoBehaviour
{
    private const int DEFAULT_MIN_AMOUNT_TO_SPAWN = 4;

    private const int DEFAULT_MAX_AMOUNT_TO_SPAWN = 7;

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
    [SerializeField] private List<TextFloatUp> _floatingTexts = new();
    [SerializeField] private CharacterTooltip charTooltip;
    [SerializeField] private GameObject _instructionsUI;
    [SerializeField] private Button _startBattleButton;
    [SerializeField] private Transform _actionsParent;
    [SerializeField] private GameObject bottomUIObjects;
    [SerializeField] private ParticleSystem bloodEffect;
    [SerializeField] private ParticleSystem armorHitEffect;

    [Header("UI")]
    [SerializeField] private Transform _healthBarParent;
    [SerializeField] private MiniStatBar _miniStatBarPrefab;
    [SerializeField] private GameObject _instructionsP1;
    [SerializeField] private GameObject _instructionsP2;
    [SerializeField] private Button _nextInstructionsButton;
    [SerializeField] private Button _showInstructionsButton;
    [SerializeField] private RectTransform _pawnPointer;
    [SerializeField] private Transform _pawnEffectsParent;
    [SerializeField] private Image _pawnEffectLargePrefab;
    [SerializeField] private ActionButton actionButton1;
    [SerializeField] private ActionButton actionButton2;
    [SerializeField] private ActionButton actionButton3;
    [SerializeField] private ActionButton actionButton4;

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
    private Tile _hoveredTile;

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

        // UI
        _endTurnButton.onClick.AddListener(EndTurn);
        _startBattleButton.onClick.AddListener(ToggleInstructions);
        _nextInstructionsButton.onClick.AddListener(NextInstructions);
        _showInstructionsButton.onClick.AddListener(ToggleInstructions);

        // show instructions
        _instructionsUI.SetActive(true);

        // add action buttons to loop
        // update the instantiation stuff in UpdatePawnUI whatever so it just uses the existing buttons
        actionButtons.Add(actionButton1);
        actionButtons.Add(actionButton2);
        //actionButtons.Add(actionButton3);
        //actionButtons.Add(actionButton4);
    }

    private void Start()
    {
        _battleResult = BattleResult.Undecided;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleInstructions();
        }

        if (Input.GetKeyDown(KeyCode.F) && CurrentPawn.OnPlayerTeam)
        {
            EndTurn();
        }

        // a better way to do this, for sure, would be on click method in tile 
        //if (Input.GetMouseButtonDown(0) && _hoveredTile != null && CurrentPawn != null && CurrentPawn.OnPlayerTeam && CurrentAction == null)
        //{
        //    _selectionManager.SetSelectedTile(CurrentPawn.CurrentTile);
        //}

        if (Input.GetMouseButtonDown(1))
        {
            ClearSelectedAction();
            //_selectionManager.ClearSelectedTile();
        }

    }

    private void OnDestroy()
    {
        Tile.OnTileHoverStart.RemoveListener(HandleTileHoverStart);
        Tile.OnTileHoverEnd.RemoveListener(HandleTileHoverEnd);

        // UI
        _endTurnButton.onClick.RemoveListener(EndTurn);
        gameOverButton.onClick.RemoveListener(ExitBattle);
        _startBattleButton.onClick.RemoveListener(ToggleInstructions);
        _nextInstructionsButton.onClick.AddListener(NextInstructions);
        _showInstructionsButton.onClick.RemoveListener(ToggleInstructions);
    }

    #region UI

    private void OnHoverInitPawnPreview(Pawn p)
    {
        Vector3 objScreenPos = CameraManager.MainCamera.WorldToScreenPoint(p.transform.position);
        objScreenPos.y += 150;

        // Convert screen position to a position relative to the UI's canvas
        Vector2 uiPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _pawnPointer.parent as RectTransform,
            objScreenPos,
            CameraManager.MainCamera,
            out uiPos);

        _pawnPointer.gameObject.SetActive(true);
        _pawnPointer.anchoredPosition = uiPos;
    }

    private void HandleEndHoverInitPawnPreview()
    {
        _pawnPointer.gameObject.SetActive(false);
    }

    private void NextInstructions()
    {
        _instructionsP1.SetActive(!_instructionsP1.activeInHierarchy);
        _instructionsP2.SetActive(!_instructionsP2.activeInHierarchy);
    }

    private void ToggleInstructions()
    {
        _instructionsUI.SetActive(!_instructionsUI.activeInHierarchy);
        if (_turnNumber == -1)
        {
            StartBattle();
        }

        if (_instructionsUI.activeInHierarchy)
        {
            _selectionManager.DisablePlayerControls();
        }
        else
        {
            _selectionManager.EnablePlayerControls();
        }
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

    public void RefreshInitStackUI()
    {
        for (int i = 0; i < _initStackParent.childCount; i++)
        {
            Transform child = _initStackParent.GetChild(i);

            child.GetComponent<PawnHeadPreview>().OnPawnPreviewHoverStart.RemoveListener(OnHoverInitPawnPreview);
            child.GetComponent<PawnHeadPreview>().OnPawnPreviewHoverEnd.RemoveListener(HandleEndHoverInitPawnPreview);

            // return to object pool
            child.gameObject.SetActive(false);
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

            GameObject pawnPreview = ObjectPool.instance.GetPawnPreview();
            pawnPreview.SetActive(true);
            pawnPreview.transform.SetParent(_initStackParent);
            // for some reason, the pawn previews get really mega stretched out.
            pawnPreview.transform.localScale = Vector3.one;
            pawnPreview.GetComponent<PawnHeadPreview>().SetData(p);

            pawnPreview.GetComponent<PawnHeadPreview>().OnPawnPreviewHoverStart.AddListener(OnHoverInitPawnPreview);
            pawnPreview.GetComponent<PawnHeadPreview>().OnPawnPreviewHoverEnd.AddListener(HandleEndHoverInitPawnPreview);

            newChildCount++;
        }
    }

    private void UpdateEffects(List<EffectData> effects)
    {
        for (int i = 0; i < _pawnEffectsParent.childCount; i++)
        {
            Destroy(_pawnEffectsParent.GetChild(i).gameObject);
        }

        foreach (EffectData effect in effects)
        {
            Image newIcon = Instantiate(_pawnEffectLargePrefab, _pawnEffectsParent);
            newIcon.sprite = effect.effectSprite;
            newIcon.GetComponent<EffectIcon>().SetData(effect);
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

            if (_currentAction != null)
            {
                apBar.SetBar(Pawn.BASE_ACTION_POINTS, p.ActionPoints, p.GetAPAfterAction(_currentAction));
                motBar.SetBar(p.GameChar.GetBattleMotivationCap(), p.Motivation, p.GetMTAfterAction(_currentAction));
            }
            else
            {
                apText.text = "AP: " + p.ActionPoints + "/" + Pawn.BASE_ACTION_POINTS;
                apBar.SetBar(Pawn.BASE_ACTION_POINTS, p.ActionPoints);
                motText.text = "MT: " + p.Motivation + "/" + p.MaxMotivation;
                motBar.SetBar(p.MaxMotivation, p.Motivation);
            }

            currentPawnPreview.SetData(p);
            //currentPawnFace.sprite = p.GetFaceSprite();
            //currentPawnHelm.sprite = p.GetFaceSprite();

            RefreshInitStackUI();

            //actionButtons.Clear();

            actionButton1.SetSelected(false);
            actionButton2.SetSelected(false);

            if (p.OnPlayerTeam)
            {
                if (actionButton1.Action != p.GameChar.WeaponItem.baseAction)
                {
                    actionButton1.SetDataButton(p.GameChar.WeaponItem.baseAction, HandleActionClicked, KeyCode.Alpha1);
                }

                if (p.GameChar.WeaponItem.specialAction != null)
                {
                    if (!actionButton2.gameObject.activeInHierarchy)
                    {
                        actionButton2.gameObject.SetActive(true);
                    }

                    if (actionButton2.Action != p.GameChar.WeaponItem.specialAction)
                    {
                        actionButton2.SetDataButton(p.GameChar.WeaponItem.specialAction, HandleActionClicked, KeyCode.Alpha2);
                    }
                }
                else
                {
                    if (actionButton2.gameObject.activeInHierarchy)
                    {
                        actionButton2.gameObject.SetActive(false);
                    }
                }
            }

            UpdateEffects(p.CurrentEffects);
        }
    }

    public void HandleTileHoverStart(Tile targetTile)
    {
        _hoveredTile = targetTile;
        Pawn hoveredPawn = targetTile.GetPawn();

        if (_selectionManager.SelectedTile != null)
        {
            //Pawn selectedPawn = _selectionManager.SelectedTile.GetPawn();    

            if (_currentPawn != null)
            {
                if (_currentAction != null && targetTile.GetTileDistance(_currentPawn.CurrentTile) <= _currentAction.range)// ;// && targetPawn.OnPlayerTeam != _currentPawn.OnPlayerTeam && targetPawn.IsTargetInRange(_currentPawn, _currentAction))
                {
                    tilesToHighlight.Clear();
                    tilesToHighlight.Add(targetTile);
                    if (_currentAction != null)
                    {
                        if (_currentAction.attackStyle == ActionData.AttackStyle.LShape)
                        {
                            tilesToHighlight.Add(_currentPawn.CurrentTile.GetClockwiseNextTile(targetTile));
                        }
                    }

                    foreach (Tile t in tilesToHighlight)
                    {
                        t.HighlightForAction();
                    }

                    UpdateUIForPawn(_currentPawn);
                }

                // if targetPawn is null, then we're hovering a movement tile.
                if (hoveredPawn == null)
                {
                    int expectedAPAfterMove = _currentPawn.GetAPAfterMove(targetTile);
                    apBar.SetBar(Pawn.BASE_ACTION_POINTS, _currentPawn.ActionPoints, expectedAPAfterMove);
                    motBar.SetBar(_currentPawn.GameChar.GetBattleMotivationCap(), _currentPawn.Motivation);
                }
            }
        }

        if (hoveredPawn != null)
        {
            StopCoroutine(OpenTooltipAfterPause());
            StartCoroutine(OpenTooltipAfterPause());
        }
    }

    private IEnumerator OpenTooltipAfterPause()
    {
        yield return new WaitForSeconds(.25f);
        ShowTooltipForPawn();
    }


    public void HandleTileHoverEnd(Tile t)
    {
        HideHitChance();

        foreach (Tile highlightedTile in tilesToHighlight)
        {
            highlightedTile.ClearActionHighlight();
        }
        tilesToHighlight.Clear();

        _hoveredTile = null;
    }

    public void HideHitChance()
    {
        hitChanceText.gameObject.SetActive(false);
        charTooltip.Hide();
    }

    public void ShowTooltipForPawn()
    {
        if (_hoveredTile == null)
        {
            return;
        }

        Pawn hoveredPawn = _hoveredTile.GetPawn();
        if (hoveredPawn == null)
        {
            return;
        }

        charTooltip.SetPawn(hoveredPawn);

        if (CurrentAction != null && CurrentPawn.OnPlayerTeam && CurrentPawn.IsTargetInRange(hoveredPawn, CurrentAction))
        {
            float chance = CurrentPawn.GetHitChance(hoveredPawn);

            if (CurrentAction != null)
            {
                ActionData attackAction = CurrentAction;

                int arDamage;
                int hpDamage;
                if (hoveredPawn.ArmorPoints > 0)
                {
                    arDamage = CurrentPawn.GameChar.GetWeaponArmorDamageForAction(attackAction);
                    hpDamage = CurrentPawn.GameChar.GetWeaponPenetrationDamageForAction(attackAction);
                }
                else
                {
                    arDamage = 0;
                    hpDamage = CurrentPawn.GameChar.GetWeaponDamageForAction(attackAction);
                }

                // have to account for armor too
                charTooltip.ShowHitPreview(chance, hpDamage, arDamage);
            }
        }
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

    #region BattleManagement

    public void ClearSelectedAction()
    {
        foreach (ActionButton abutton in actionButtons)
        {
            if (abutton.IsSelected)
            {
                abutton.SetInactive();
            }
        }

        if (_currentAction != null)
        {
            //CurrentPawn.CurrentTile.HighlightTilesInRange(_currentAction.range + 1, false, Tile.TileHighlightType.AttackRange);
            _currentAction = null;

            _selectionManager.SetIdleMode(true);

            //if (CurrentPawn.HasActionsRemaining())
            //{
            //    CurrentPawn.CurrentTile.HighlightTilesInRange(_currentPawn.MoveRange+1, true, Tile.TileHighlightType.Move);
            //}
        }

        ShowTooltipForPawn();
        UpdateUIForPawn(_currentPawn);
    }

    private void HandleActionClicked(ActionData action)
    {
        ClearSelectedAction();

        _currentAction = action;

        _selectionManager.SetIdleMode(false);

        //_selectionManager.SetSelectedTile(CurrentPawn.CurrentTile);

        //CurrentPawn.CurrentTile.HighlightTilesInRange(_currentPawn.MoveRange+1, false, Tile.TileHighlightType.Move);
        //CurrentPawn.CurrentTile.HighlightTilesInRange(_currentAction.range+1, true, Tile.TileHighlightType.AttackRange);

        //OnActionUpdated.Invoke(action);

        ShowTooltipForPawn();
        UpdateUIForPawn(_currentPawn);
    }

    private void EndTurn()
    {
        Pawn activePawn = _selectionManager.SelectedTile.GetPawn();
        PawnFinished(activePawn);
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
        p.HandleTurnEnded();

        if (_selectionManager.SelectedTile != null)
        {
            _selectionManager.SelectedTile.SetSelected(false);
        }

        NextActivation();
    }

    /// <summary>
    /// When a pawn acts but not necessarily when it's finished
    /// </summary>
    /// <param name="p"></param>
    public void PawnActivated(Pawn p)
    {
        UpdateUIForPawn(p);
        //_selectionManager.SetSelectedTile(p.CurrentTile);

        // update selected tile after a potential move
        _selectionManager.SetSelectedTile(CurrentPawn.CurrentTile);

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
        
        pawnList = pawnList.OrderBy(pawn => pawn.Initiative).ToList();

        // this way the stack can be sorted properly 
        _initiativeStack = new(pawnList);
    }

    private void StartBattle()
    {
        _turnNumber = 0;
        _instructionsUI.SetActive(false);
        NextActivation();
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

    public void NextActivation()
    {
        if (_currentPawn != null)
        {
            _currentPawn.OnEffectUpdate.RemoveListener(UpdateEffects);
        }

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
                //if (_currentPawn.CurrentTile == null)
                //{
                //    yield return new WaitUntil(() => _currentPawn.CurrentTile != null);
                //}

                _currentPawn.HandleActivation();
                UpdateUIForPawn(_currentPawn);
                _currentPawn.OnEffectUpdate.AddListener(UpdateEffects);

                _selectionManager.HandleTurnChange(_currentPawn.OnPlayerTeam);
                _endTurnButton.gameObject.SetActive(_currentPawn.OnPlayerTeam);

                if (!_currentPawn.OnPlayerTeam)
                //{
                    //_selectionManager.SetSelectedTile(_currentPawn.CurrentTile);
                //}
                //else
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

    #endregion
}
