using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    private const int DEFAULT_AMOUNT_TO_SPAWN = 4;

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
    [SerializeField] GameObject characterInfoPanel;
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
    [SerializeField] private GameObject _initStackGO;
    [SerializeField] private Transform _initStackParent;
    [SerializeField] GameObject pawnPreviewPrefab;
    [SerializeField] private List<TextFloatUp> _floatingTexts = new();
    [SerializeField] private CharacterTooltip charTooltip;
    [SerializeField] private GameObject _instructionsUI;
    [SerializeField] private Button _startBattleButton;
    [SerializeField] private WeaponItemData testWeapon;

    private Stack<Pawn> _initiativeStack = new ();

    public int TurnNumber => _turnNumber;
    private int _turnNumber = -1;

    private void Awake()
    {
        _instance = this;
        gameOverButton.onClick.AddListener(ExitBattle);

        int enemiesToSpawn;
        if (GameManager.Instance != null)
        {
            enemiesToSpawn = GameManager.Instance.GetNumOfEnemiesToSpawn();

            Pawn playerPawn = Instantiate(pawnPrefab, friendlyParent).GetComponent<Pawn>();
            _playerPawns.Add(playerPawn);
            playerPawn.SetCharacter(GameManager.Instance.PlayerCharacter, true);

            foreach (GameCharacter character in GameManager.Instance.PlayerFollowers)
            {
                Pawn newPawn = Instantiate(pawnPrefab, friendlyParent).GetComponent<Pawn>();
                _playerPawns.Add(newPawn);
                newPawn.SetCharacter(character, true);
            }
        }
        else
        {
            Debug.Log("No game manager, spawning default amount");
            enemiesToSpawn = DEFAULT_AMOUNT_TO_SPAWN;

            // spawn some friendlies
            for (int i = 0; i < DEFAULT_AMOUNT_TO_SPAWN; i++)
            {
                Pawn newPawn = Instantiate(pawnPrefab, friendlyParent).GetComponent<Pawn>();
                _playerPawns.Add(newPawn);

                GameCharacter testCharacter = new GameCharacter();
                testCharacter.EquipItem(testWeapon);

                newPawn.SetCharacter(testCharacter, true);
            }
        }

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Pawn newPawn = Instantiate(pawnPrefab, enemyParent).GetComponent<Pawn>();

            GameCharacter guy = new();
            guy.EquipItem(testWeapon);
            newPawn.SetCharacter(guy, false);

            _enemyAI.RegisterPawn(newPawn);
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
            characterInfoPanel.SetActive(true);
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

            _initStackGO.SetActive(true);
            RefreshInitStackUI();
        }
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
            // if targetPawn is null, then we're hovering a movement tile.
            if (targetPawn == null)
            {
                int expectedAPAfterMove = selectedPawn.GetAPAfterMove(targetTile);
                apBar.SetBar(Pawn.BASE_ACTION_POINTS, selectedPawn.ActionPoints, expectedAPAfterMove);

            }
            else
            {
                ShowTooltipForPawn(targetPawn);
                if (selectedPawn.OnPlayerTeam != targetPawn.OnPlayerTeam)
                {
                    apBar.SetBar(Pawn.BASE_ACTION_POINTS, selectedPawn.ActionPoints, selectedPawn.GetAPAfterAttack());
                }
                else
                {
                    // otherwise we might be hovering ourselves or a teammate so reset the AP Bar
                    apBar.SetBar(Pawn.BASE_ACTION_POINTS, selectedPawn.ActionPoints);
                }
            }
        }
    }

    public void HandleTileHoverEnd(Tile t)
    {
        HideHitChance();
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
            charTooltip.ShowHitPreview(chance, selectedPawn.Damage);
        }
    }


    private void ExitBattle()
    {
        bool playerWon = _battleResult == BattleResult.Win;

        GameManager.Instance.ExitBattle(playerWon);
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
        characterInfoPanel.SetActive(false);
        _initStackGO.SetActive(false);
        winLoseText.text = battleResult == BattleResult.Win ? "Victory!" : "Defeat!" ;
        _battleResult = battleResult;
    }

    public IEnumerator NextActivation()
    {
        Pawn curentPawn = GetNextPawn();

        // see if the battle is over. If so, do sumthin about it 
        if (_enemyAI.GetLivingPawns().Count <= 0)
        {
            HandleBattleResult(BattleResult.Win);
        }
        else
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

            if (alivePawns <= 0)
            {
                HandleBattleResult(BattleResult.Lose);
            }
            else
            {
                if (curentPawn.CurrentTile == null)
                {
                    yield return new WaitUntil(() => curentPawn.CurrentTile != null);
                }

                curentPawn.HandleActivation();
                UpdateUIForPawn(curentPawn);
                _selectionManager.HandleTurnChange(curentPawn.OnPlayerTeam);
                _endTurnButton.gameObject.SetActive(curentPawn.OnPlayerTeam);

                if (curentPawn.OnPlayerTeam)
                {
                    _selectionManager.SetSelectedTile(curentPawn.CurrentTile);
                }
                else
                {
                    _enemyAI.DoTurn(curentPawn);
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
