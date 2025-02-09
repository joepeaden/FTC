using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Threading.Tasks;

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
    [SerializeField] PawnPreview currentPawnPreview;
    [SerializeField] TMP_Text armorText;
    [SerializeField] PipStatBar armorBar;
    [SerializeField] TMP_Text healthText;
    [SerializeField] PipStatBar healthBar;
    [SerializeField] TMP_Text apText;
    [SerializeField] PipStatBar apBar;
    [SerializeField] TMP_Text motText;
    [SerializeField] PipStatBar motBar;
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
    [SerializeField] private List<ActionButton> _actionButtons = new();

    [Header("Equipment")]
    [SerializeField] private ArmorItemData lightHelm;
    [SerializeField] private ArmorItemData medHelm;
    [SerializeField] private ArmorItemData heavyHelm;
    [SerializeField] private WeaponItemData club;
    [SerializeField] private WeaponItemData sword;
    [SerializeField] private WeaponItemData axe;
    [SerializeField] private WeaponItemData spear;

    [Header("Audio")]
    [SerializeField] private AudioClip _levelUpSound;
    /// <summary>
    /// This is just a ref for audio sources retrieved from the object pooler.
    /// It's needed because sometimes we want to disable the audio source after
    /// an animation, which is probably out of the scope in which it was
    /// retrieved.
    /// </summary>
    /// <remarks>
    /// It may be worth creating a script for the audio source objects which
    /// handles this.
    /// </remarks>
    private GameObject pooledAudioSourceGO;

    public Pawn CurrentPawn => _currentPawn;
    private Pawn _currentPawn;
    private Tile _hoveredTile;

    private Stack<Pawn> _initiativeStack = new ();

    public int TurnNumber => _turnNumber;
    private int _turnNumber = -1;

    private List<Tile> tilesToHighlight = new();

    private void Awake()
    {
        _instance = this;

        gameOverButton.onClick.AddListener(ExitBattle);

        // if not started from Battle scene, spawn player's company and enemies in contract
        if (GameManager.Instance != null)
        {
            //dudesToSpawn = GameManager.Instance.GetNumOfEnemiesToSpawn();

            foreach (GameCharacter character in GameManager.Instance.PlayerFollowers)
            {
                Pawn newPawn = Instantiate(pawnPrefab, friendlyParent).GetComponent<Pawn>();
                _playerPawns.Add(newPawn);
                newPawn.SetCharacter(character);

                MiniStatBar miniStats = Instantiate(_miniStatBarPrefab, _healthBarParent);
                miniStats.SetData(newPawn);
            }

            foreach(GameCharacter character in GameManager.Instance.GetEnemiesForContract())
            {
                Pawn newPawn = Instantiate(pawnPrefab, enemyParent).GetComponent<Pawn>();
                newPawn.SetCharacter(character);

                _enemyAI.RegisterPawn(newPawn);

                MiniStatBar miniStats = Instantiate(_miniStatBarPrefab, _healthBarParent);
                miniStats.SetData(newPawn);
            }
        }
        // otherwise, spawn a random assortment of friendly and enemy dudes
        else
        {
            // change game over button text to "restart"
            gameOverButton.GetComponentInChildren<TMP_Text>().text = "Restart";

            Debug.Log("No game manager, spawning default amount");
            SpawnTestGuys(true);

            SpawnTestGuys(false);
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
    }

    private void Start()
    {
        _battleResult = BattleResult.Undecided;
        StartBattle();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //ToggleInstructions();
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

            child.GetComponent<PawnPreview>().OnPawnPreviewHoverStart.RemoveListener(OnHoverInitPawnPreview);
            child.GetComponent<PawnPreview>().OnPawnPreviewHoverEnd.RemoveListener(HandleEndHoverInitPawnPreview);

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
            pawnPreview.GetComponent<PawnPreview>().SetData(p);

            pawnPreview.GetComponent<PawnPreview>().OnPawnPreviewHoverStart.AddListener(OnHoverInitPawnPreview);
            pawnPreview.GetComponent<PawnPreview>().OnPawnPreviewHoverEnd.AddListener(HandleEndHoverInitPawnPreview);

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
            characterMotivatorText.text = p.CurrentMotivator.ToString();
            armorText.text = "AR: " + p.ArmorPoints + "/" + p.MaxArmorPoints;
            armorBar.SetBar(p.ArmorPoints);
            healthText.text = "HP: " + p.HitPoints + "/" + p.MaxHitPoints;
            healthBar.SetBar(p.HitPoints);

            if (Ability.SelectedAbility != null)
            {
                apBar.SetBar(p.ActionPoints);
                motBar.SetBar(p.Motivation);
            }
            else
            {
                apText.text = "AP: " + p.ActionPoints + "/" + Pawn.BASE_ACTION_POINTS;
                apBar.SetBar(p.ActionPoints);
                motText.text = "MT: " + p.Motivation + "/" + p.MaxMotivation;
                motBar.SetBar(p.Motivation);
            }

            currentPawnPreview.SetData(p);

            RefreshInitStackUI();

            List <Ability> pawnAbilities = CurrentPawn.GetAbilities();
            // there's currently only 4 ability buttons - will need to address that at some point,
            // could cause problems.
            int i = 0;
            for (; i < pawnAbilities.Count; i++)
            {
                ActionButton actionButton = _actionButtons[i];

                actionButton.SetSelected(false);
                actionButton.gameObject.SetActive(p.OnPlayerTeam);

                if (p.OnPlayerTeam)
                {
                    if (actionButton.TheAbility != pawnAbilities[i])
                    {
                        actionButton.SetDataButton(pawnAbilities[i], HandleActionClicked, i+1);
                    }
                }
            }

            // update the remaining buttons
            for (; i < _actionButtons.Count; i++)
            {
                ActionButton actionButton = _actionButtons[i];

                actionButton.SetSelected(false);
                actionButton.gameObject.SetActive(false);
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
            if (_currentPawn != null)
            {
                if (Ability.SelectedAbility != null && targetTile.GetTileDistance(_currentPawn.CurrentTile) <= Ability.SelectedAbility.GetData().range)
                {
                    tilesToHighlight.Clear();
                    tilesToHighlight.Add(targetTile);
                    
                    if ((ActionData)Ability.SelectedAbility.GetData() as ActionData != null && ((ActionData)Ability.SelectedAbility.GetData()).attackStyle == ActionData.AttackStyle.LShape)
                    {
                        tilesToHighlight.Add(_currentPawn.CurrentTile.GetClockwiseNextTile(targetTile));
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
                    apBar.SetBar(expectedAPAfterMove);
                    motBar.SetBar(_currentPawn.Motivation);
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

        //if ((ActionData)Ability.SelectedAbility.GetData() as ActionData)
        //{
        //    if (Ability.SelectedAbility != null && CurrentPawn.OnPlayerTeam && CurrentPawn.IsTargetInRange(hoveredPawn, Ability.SelectedAbility))
        //    {
        //        float chance = CurrentPawn.GetHitChance(hoveredPawn);

        //        ActionData attackAction = ((ActionData)Ability.SelectedAbility.GetData());

        //        int arDamage;
        //        int hpDamage;
        //        if (hoveredPawn.ArmorPoints > 0)
        //        {
        //            arDamage = CurrentPawn.GameChar.GetWeaponArmorDamageForAction(attackAction);
        //            hpDamage = CurrentPawn.GameChar.GetWeaponPenetrationDamageForAction(attackAction);
        //        }
        //        else
        //        {
        //            arDamage = 0;
        //            hpDamage = CurrentPawn.GameChar.GetWeaponDamageForAction(attackAction);
        //        }

        //        // have to account for armor too
        //        charTooltip.ShowHitPreview(chance, hpDamage, arDamage);
        //    }
        //}
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

    public void SpawnTestGuys(bool friendly)
    {
        int numToSpawn = Random.Range(DEFAULT_MIN_AMOUNT_TO_SPAWN, DEFAULT_MAX_AMOUNT_TO_SPAWN);

        for (int i = 0; i < numToSpawn; i++)
        {
            Pawn newPawn = Instantiate(pawnPrefab, friendly ? friendlyParent : enemyParent).GetComponent<Pawn>();

            GameCharacter guy = new(friendly);

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

            newPawn.SetCharacter(guy);

            if (friendly)
            {
                _playerPawns.Add(newPawn);
            }
            else
            {
                _enemyAI.RegisterPawn(newPawn);
            }

            MiniStatBar miniStats = Instantiate(_miniStatBarPrefab, _healthBarParent);
            miniStats.SetData(newPawn);
        }
    }

    public void ClearSelectedAction()
    {
        foreach (ActionButton abutton in _actionButtons)
        {
            if (abutton.IsSelected)
            {
                abutton.SetInactive();
            }
        }

        if (Ability.SelectedAbility != null)
        {
            //CurrentPawn.CurrentTile.HighlightTilesInRange(_currentAction.range + 1, false, Tile.TileHighlightType.AttackRange);
            Ability.SelectedAbility = null;

            _selectionManager.SetIdleMode(true);

            //if (CurrentPawn.HasActionsRemaining())
            //{
            //    CurrentPawn.CurrentTile.HighlightTilesInRange(_currentPawn.MoveRange+1, true, Tile.TileHighlightType.Move);
            //}
        }

        ShowTooltipForPawn();
        UpdateUIForPawn(_currentPawn);
    }

    private void HandleActionClicked(Ability action)
    {
        ClearSelectedAction();

        Ability.SelectedAbility = action;

        _selectionManager.SetIdleMode(false);

        //_selectionManager.SetSelectedTile(CurrentPawn.CurrentTile);s

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

            // distribute XP for participating in battle
            for (int i = 0; i < _playerPawns.Count; i++)
            {
                _playerPawns[i].GameChar.AddXP(1);
            }

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
        StartCoroutine(PawnActivatedCoroutine(p));
    }

    private void OnLevelUpAudioFinished()
    {
        // return to object pool
        pooledAudioSourceGO.SetActive(false);
    }

    public IEnumerator PawnActivatedCoroutine(Pawn p)
    {
        // level up visuals & audio. Need pauses to allow the player time to
        // process what's going on.
        if (p.PendingLevelUp)
        {
            p.PendingLevelUp = false;

            yield return new WaitForSeconds(.3f);

            p.TriggerLevelUpVisuals();

            yield return new WaitForSeconds(.5f);

            pooledAudioSourceGO = ObjectPool.instance.GetAudioSource();
            pooledAudioSourceGO.SetActive(true);
            AudioSource audioSource = pooledAudioSourceGO.GetComponent<AudioSource>();
            audioSource.clip = _levelUpSound;
            audioSource.Play();

            Invoke("OnLevelUpAudioFinished", _levelUpSound.length);

            BattleManager.Instance.AddTextNotification(p.transform.position, "Level up!");

            yield return new WaitForSeconds(.25f);
        }

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

    public async void NextActivation()
    {
        // pause a little bit so the player can keep track of what the heck is happening
        await Task.Delay(250);

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
                _currentPawn.HandleActivation();
                UpdateUIForPawn(_currentPawn);
                _currentPawn.OnEffectUpdate.AddListener(UpdateEffects);

                _selectionManager.HandleTurnChange(_currentPawn.OnPlayerTeam);
                _endTurnButton.gameObject.SetActive(_currentPawn.OnPlayerTeam);

                if (!_currentPawn.OnPlayerTeam)
                {
                    _enemyAI.DoTurn(_currentPawn);
                    _selectionManager.DisablePlayerControls();
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
