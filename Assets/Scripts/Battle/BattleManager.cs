using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    [SerializeField] private AIPlayer _aiPlayer;
    [SerializeField] private Button gameOverButton;
    [SerializeField] private MyInputManager _selectionManager;
    [SerializeField] GameObject pawnPrefab;
    [SerializeField] Transform enemyParent;
    [SerializeField] Transform friendlyParent;
    [SerializeField] TMP_Text hitChanceText;
    [SerializeField] TMP_Text characterNameText;
    [SerializeField] TMP_Text characterMotivatorText;
    [SerializeField] PawnPreview currentPawnPreview;
    [SerializeField] PipStatBar armorBar;
    [SerializeField] PipStatBar healthBar;
    [SerializeField] PipStatBar apBar;
    [SerializeField] PipStatBar motBar;
    [SerializeField] private Transform _initStackParent;
    [SerializeField] private List<TextNotificationStack> _textNotifs;
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
    [SerializeField] private List<InfoLine> _motivationConditionDisplay = new();

    [Header("Equipment")]
    [SerializeField] private ArmorItemData lightHelm;
    [SerializeField] private ArmorItemData medHelm;
    [SerializeField] private ArmorItemData heavyHelm;
    [SerializeField] private WeaponItemData club;
    [SerializeField] private WeaponItemData sword;
    [SerializeField] private WeaponItemData axe;
    [SerializeField] private WeaponItemData spear;
    [SerializeField] private WeaponItemData bigSword;

    [Header("Audio")]
    [SerializeField] private AudioClip _levelUpSound;

    public Pawn CurrentPawn => _currentPawn;
    private Pawn _currentPawn;

    private Stack<Pawn> _initiativeStack = new ();

    private int _turnNumber = -1;

    private List<(string, Color)> pendingTextNotifs = new();

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

            foreach (GameCharacter character in GameManager.Instance.GetEnemiesForContract())
            {
                Pawn newPawn = Instantiate(pawnPrefab, enemyParent).GetComponent<Pawn>();
                newPawn.SetCharacter(character);

                _aiPlayer.RegisterPawn(newPawn);

                MiniStatBar miniStats = Instantiate(_miniStatBarPrefab, _healthBarParent);
                miniStats.SetData(newPawn);
            }
        }
        // otherwise, spawn a random assortment of friendly and enemy dudes
        else
        {
            StartCoroutine(TestModeOnDataLoadedStart());
        }

        // UI
        _endTurnButton.onClick.AddListener(EndTurn);
        _startBattleButton.onClick.AddListener(ToggleInstructions);
        _nextInstructionsButton.onClick.AddListener(NextInstructions);
        _showInstructionsButton.onClick.AddListener(ToggleInstructions);

        // show instructions
        //_instructionsUI.SetActive(true);
    }

    /// <summary>
    /// Start for when just testing battles. Not very secure but it's just for testing. 
    /// </summary>
    /// <returns></returns>
    private IEnumerator TestModeOnDataLoadedStart()
    {
        // instantiate data loader
        DataLoader dataLoader = new DataLoader();
        dataLoader.LoadData();

        // wait for data to load. 
        yield return new WaitForSeconds(3f);
        // change game over button text to "restart"
        gameOverButton.GetComponentInChildren<TMP_Text>().text = "Restart";

        Debug.Log("No game manager, spawning default amount");
        SpawnTestGuys(true);
        SpawnTestGuys(false);

        _battleResult = BattleResult.Undecided;
        StartBattle();
    }

    private void Start()
    {
        GridGenerator.Instance.AddTileHoverListeners(HandleTileHoverStart, HandleTileHoverEnd);

        if (GameManager.Instance != null)
        {
            _battleResult = BattleResult.Undecided;
            StartBattle();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && CurrentPawn.OnPlayerTeam)
        {
            EndTurn();
        }

        if (Input.GetMouseButtonDown(1))
        {
            ClearSelectedAction();
        }

    }

    private void OnDestroy()
    {
        GridGenerator.Instance.RemoveTileHoverListeners(HandleTileHoverStart, HandleTileHoverEnd);
        
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
            _selectionManager.PlayerControlsActive = false;
        }
        else
        {
            _selectionManager.PlayerControlsActive = true;
        }
    }

    public void AddPendingTextNotification(string str, Color color)
    {
        pendingTextNotifs.Add((str, color));
    }
    
    public void TriggerTextNotification(Vector3 pos)
    {
        foreach (TextNotificationStack txt in _textNotifs)
        {
            if (txt.InUse)
            {
                continue;
            }
            else
            {
                txt.SetData(pos, pendingTextNotifs);
                break;
            }
        }

        pendingTextNotifs.Clear();
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
            armorBar.SetBar(p.ArmorPoints);
            healthBar.SetBar(p.HitPoints);

            if (Ability.SelectedAbility != null)
            {
                apBar.SetBar(p.actionPoints);
                motBar.SetBar(p.Motivation);
            }
            else
            {
                apBar.SetBar(p.actionPoints);
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

            // this conditions list could be its own class and prefab, such that
            // it can just be dragged and dropped anywhere. Right now identical
            // code is found in CharDetailPanel.
            List<MotCondData> motConditions = p.GameChar.GetMotCondsForBattle();
            i = 0;
            for (; i < motConditions.Count; i++)
            {
                MotCondData condition = motConditions[i];

                _motivationConditionDisplay[i].SetData("", condition.description);
            }

            // update the remaining buttons
            for (; i < _motivationConditionDisplay.Count; i++)
            {
                _motivationConditionDisplay[i].Hide();
            }
        }
    }

    public void HandleTileHoverStart(Tile targetTile)
    {
        Pawn hoveredPawn = targetTile.GetPawn();

        if (_currentPawn != null)
        {
            UpdateUIForPawn(_currentPawn);
        }

        _selectionManager.UpdateTileHovered(targetTile);

        if (hoveredPawn != null)
        {
            StopCoroutine(OpenTooltipAfterPause(hoveredPawn));
            StartCoroutine(OpenTooltipAfterPause(hoveredPawn));
        }
    }

    private IEnumerator OpenTooltipAfterPause(Pawn hoveredPawn)
    {
        yield return new WaitForSeconds(.25f);
        ShowTooltipForPawn(hoveredPawn);
    }

    public void HandleTileHoverEnd(Tile t)
    {
        HideHitChance();

        _selectionManager.HoveredTile = null;
    }

    public void HideHitChance()
    {
        hitChanceText.gameObject.SetActive(false);
        charTooltip.Hide();
    }

    public void ShowTooltipForPawn(Pawn hoveredPawn)
    {
        if (hoveredPawn == null)
        {
            return;
        }

        charTooltip.SetPawn(hoveredPawn);
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
        int numToSpawn = Random.Range(DEFAULT_MIN_AMOUNT_TO_SPAWN, DEFAULT_MAX_AMOUNT_TO_SPAWN);// : 3;

        for (int i = 0; i < numToSpawn; i++)
        {
            Pawn newPawn = Instantiate(pawnPrefab, friendly ? friendlyParent : enemyParent).GetComponent<Pawn>();

            GameCharacter guy = new(friendly ? DataLoader.charTypes["player"] : DataLoader.charTypes["warrior"]);

                guy.ChangeHP(6);
            
            if (friendly)
            {
                // if (i == 1)// == 0)
                // {
                //     guy.EquipItem(bigSword);
                //     guy.EquipItem(heavyHelm);
                //     guy.Abilities.Add(DataLoader.abilities["firstaid"]);
                // }
                // else
                // {
                // }
                
                int roll = Random.Range(0,7);
                switch (roll)
                {
                case 0:
                    guy.Passives.Add(DataLoader.passives["holy"]);
                    guy.Abilities.Add(DataLoader.abilities["firstaid"]);
                    guy.EquipItem(medHelm);
                    guy.EquipItem(sword);
                    break;
                case 1:
                    guy.Passives.Add(DataLoader.passives["bulwark"]);
                    guy.Abilities.Add(DataLoader.abilities["defensiveposture"]);
                    guy.EquipItem(heavyHelm);
                    guy.EquipItem(bigSword);
                    break;
                case 2:
                    guy.Passives.Add(DataLoader.passives["warrior"]);
                    guy.Abilities.Add(DataLoader.abilities["shove"]);
                    guy.EquipItem(lightHelm);
                    guy.EquipItem(spear);
                    break;
                case 3:
                    guy.Passives.Add(DataLoader.passives["perfect"]);
                    guy.Abilities.Add(DataLoader.abilities["shove"]);
                    guy.EquipItem(lightHelm);
                    guy.EquipItem(sword);
                    break;
                case 4: 
                    guy.Passives.Add(DataLoader.passives["courage"]);
                    guy.Abilities.Add(DataLoader.abilities["shove"]);
                    guy.EquipItem(axe);
                    break;
                case 5: 
                    guy.Passives.Add(DataLoader.passives["tank"]);
                    guy.Abilities.Add(DataLoader.abilities["defensiveposture"]);
                    guy.EquipItem(heavyHelm);
                    guy.EquipItem(axe);
                    break;
                }
            }

            newPawn.SetCharacter(guy);

            if (friendly)
            {
                _playerPawns.Add(newPawn);
            }
            else
            {
                _aiPlayer.RegisterPawn(newPawn);
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
            _selectionManager.CurrentAbility = null;    
            Ability.SelectedAbility = null;
        }
    }

    private void HandleActionClicked(Ability action)
    {
        ClearSelectedAction();

        // I got into some very bad coding habits over time. Using statics ALOT in this project. I have come to understand just how bad this design is.
        // But, sometimes... if it works, it works. I need to make progress. So, this is part of the attempt to at least keep SelectionManager mostly decoupled.

        Ability.SelectedAbility = action;
        _selectionManager.CurrentAbility = action;

        UpdateUIForPawn(_currentPawn);
    }

    private void EndTurn()
    {
        PawnFinished(_currentPawn);
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

        StartCoroutine(NextPawnCoroutine());
    }

    /// <summary>
    /// When a pawn acts but not necessarily when it's finished
    /// </summary>
    /// <param name="p"></param>
    public void PawnActivated(Pawn p)
    {
        StartCoroutine(PawnActivatedCoroutine(p));
    }

    public IEnumerator PawnActivatedCoroutine(Pawn p)
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

            GameObject pooledAudioSourceGO = ObjectPool.instance.GetAudioSource();
            pooledAudioSourceGO.SetActive(true);
            AudioSource audioSource = pooledAudioSourceGO.GetComponent<AudioSource>();
            audioSource.clip = _levelUpSound;
            audioSource.Play();

            AddPendingTextNotification("Level up!", Color.yellow);
            TriggerTextNotification(p.transform.position);

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
        else if (p.OnPlayerTeam && p.IsPossessed)
        {
            _aiPlayer.DoTurn(p);
        }
        else if (!p.OnPlayerTeam)
        {
            _aiPlayer.DoTurn(p);
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

        _initiativeStack.Clear();

        foreach (Pawn p in _aiPlayer.GetEnemyLivingPawns())
        {
            _initiativeStack.Push(p);
        }
        
        foreach (Pawn p in GetFriendlyLivingPawns())
        {
            _initiativeStack.Push(p);
        }
    }

    private void StartBattle()
    {
        _turnNumber = 0;
        _instructionsUI.SetActive(false);
        StartCoroutine(NextPawnCoroutine());
    }

    private void HandleBattleResult(BattleResult battleResult)
    {
        // distribute XP for participating in battle
        for (int i = 0; i < _playerPawns.Count; i++)
        {
            _playerPawns[i].HandleBattleEnd();
        }

        turnUI.SetActive(false);
        winLoseUI.SetActive(true);
        bottomUIObjects.SetActive(false);
        winLoseText.text = battleResult == BattleResult.Win ? "Victory!" : "Defeat!" ;

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
            _currentPawn.OnEffectUpdate.RemoveListener(UpdateEffects);
        }

        _currentPawn = GetNextPawn();
        _selectionManager.CurrentPawn = _currentPawn;

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
                UpdateUIForPawn(_currentPawn);
                _currentPawn.OnEffectUpdate.AddListener(UpdateEffects);

                _selectionManager.HandleTurnChange(_currentPawn.OnPlayerTeam);
                _endTurnButton.gameObject.SetActive(_currentPawn.OnPlayerTeam);

                if (!_currentPawn.OnPlayerTeam)
                {
                    _selectionManager.PlayerControlsActive = false;
                    _aiPlayer.DoTurn(_currentPawn);
                }        
                else if (_currentPawn.OnPlayerTeam && _currentPawn.GameChar.RollPosessed())
                {
                    _selectionManager.PlayerControlsActive = false;
                    _currentPawn.IsPossessed = true;
                    AddPendingTextNotification("Possession!", Color.yellow);
                    TriggerTextNotification(_currentPawn.transform.position);
                    _aiPlayer.DoTurn(_currentPawn);
                }
                else
                {
                    _selectionManager.PlayerControlsActive = true;
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
