using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Collections;
using System.Security;

public class BattleUI : MonoBehaviour
{
    [SerializeField] private Button _endTurnButton;
    [SerializeField] TMP_Text turnText;
    [SerializeField] GameObject turnUI;
    [SerializeField] TMP_Text postBattleTitle;
    [SerializeField] GameObject postBattleScreen;
    [SerializeField] private Button gameFinishedButton;
    [SerializeField] TMP_Text characterNameText;
    [SerializeField] PawnPreview currentPawnPreview;
    [SerializeField] PipStatBar armorBar;
    [SerializeField] PipStatBar healthBar;
    [SerializeField] PipStatBar apBar;
    [SerializeField] PipStatBar motBar;
    [SerializeField] private Transform _initStackParent;
    [SerializeField] private CharacterTooltip charTooltip;
    [SerializeField] private GameObject bottomUIObjects;
    [SerializeField] private Transform _healthBarParent;
    [SerializeField] private MiniStatBar _miniStatBarPrefab;
    [SerializeField] private RectTransform _pawnPointer;
    [SerializeField] private Transform _pawnEffectsParent;
    [SerializeField] private Image _pawnEffectLargePrefab;
    [SerializeField] private List<ActionButton> _actionButtons = new(); 
    // [SerializeField] private List<InfoLine> _motivationConditionDisplay = new();

    [HideInInspector] public UnityEvent OnGameFinished = new();
    [HideInInspector] public UnityEvent OnEndTurn = new();
    
    [SerializeField] private PawnEvents _pawnEvents; 
    [SerializeField] private FlowDirector _battleManager;

    private Tile _hoveredTile;
    private List<Tile> tilesToHighlight = new();

    // really, shouldn't have selection manager here. This is just here for now to keep 
    // the refactor from becoming out of scope at the time
    [SerializeField] private SelectionManager _selectionManager;

    #region  Unity Event Methods

    private void Awake()
    {
        gameFinishedButton.onClick.AddListener(NotifyGameFinished);
        _endTurnButton.onClick.AddListener(NotifyEndTurn);

        Tile.OnTileHoverStart.AddListener(HandleTileHoverStart);
        Tile.OnTileHoverEnd.AddListener(HandleTileHoverEnd);

        _pawnEvents.AddSpawnedListener(RegisterPawn);
        _pawnEvents.AddActedListener(HandlePawnActed);
        _pawnEvents.AddKilledListener(HandlePawnKilled);
    }

    private void OnDestroy()
    {
        gameFinishedButton.onClick.RemoveListener(NotifyGameFinished);
        _endTurnButton.onClick.RemoveListener(NotifyEndTurn);

        Tile.OnTileHoverStart.RemoveListener(HandleTileHoverStart);
        Tile.OnTileHoverEnd.RemoveListener(HandleTileHoverEnd);

        _pawnEvents.RemoveSpawnedListener(RegisterPawn);
        _pawnEvents.RemoveActedListener(HandlePawnActed);
        _pawnEvents.RemoveKilledListener(HandlePawnKilled);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            ClearSelectedAction();
        }
    }

    #endregion 

    #region Events Emitters
    
    private void NotifyGameFinished() { OnGameFinished.Invoke(); }
    private void NotifyEndTurn() { OnEndTurn.Invoke(); }
    
    #endregion
    
    #region External Listeners
    
    private void RegisterPawn(Pawn pawn)
    {
        MiniStatBar miniStats = Instantiate(_miniStatBarPrefab, _healthBarParent);
        miniStats.SetData(pawn);
    }    
    
    private void HandlePawnActed(Pawn p)
    {
        UpdateUIForPawn(p);
        ClearSelectedAction();
    }

    private void HandlePawnKilled(Pawn p)
    {
        RefreshInitStackUI();
    }

    #endregion
    
    #region  Unorganized
    public void SetTurnUI(int turnNum)
    {
        turnText.text = turnNum.ToString();
    }

    public void HandleBattleResult(FlowDirector.BattleResult battleResult)
    {
        turnUI.SetActive(false);
        postBattleScreen.SetActive(true);
        bottomUIObjects.SetActive(false);
        postBattleTitle.text = battleResult == FlowDirector.BattleResult.Win ? "Victory!" : "Defeat!" ;
    }

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
    
    public void UpdateEffects(List<EffectData> effects)
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

    public void UpdateUIForPawn(Pawn p)
    {
        if (p != null)
        {
            bottomUIObjects.SetActive(true);
            characterNameText.text = p.GameChar.CharName;
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

            List <Ability> pawnAbilities = p.GetAbilities();
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
                        actionButton.SetDataButton(pawnAbilities[i], HandleActionClicked, ClearSelectedAction, i+1);
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
            // List<MotCondData> motConditions = p.GameChar.GetMotCondsForBattle();
            // i = 0;
            // for (; i < motConditions.Count; i++)
            // {
            //     MotCondData condition = motConditions[i];

            //     _motivationConditionDisplay[i].SetData("", condition.description);
            // }

            // // update the remaining buttons
            // for (; i < _motivationConditionDisplay.Count; i++)
            // {
            //     _motivationConditionDisplay[i].Hide();
            // }

            _endTurnButton.gameObject.SetActive(p.OnPlayerTeam);
        }
    }

    public void HandleTileHoverStart(Tile targetTile)
    {
        _hoveredTile = targetTile;
        Pawn hoveredPawn = targetTile.GetPawn();

        if (_selectionManager.SelectedTile != null)
        {
            if (_battleManager.CurrentPawn != null)
            {
                if (Ability.SelectedAbility != null && targetTile.GetTileDistance(_battleManager.CurrentPawn.CurrentTile) <= Ability.SelectedAbility.range)
                {
                    tilesToHighlight.Clear();
                    tilesToHighlight.Add(targetTile);
                    
                    if (Ability.SelectedAbility as WeaponAbilityData != null && ((WeaponAbilityData)Ability.SelectedAbility).attackStyle == WeaponAbilityData.AttackStyle.LShape)
                    {
                        tilesToHighlight.Add(_battleManager.CurrentPawn.CurrentTile.GetClockwiseNextTile(targetTile));
                    }

                    foreach (Tile t in tilesToHighlight)
                    {
                        t.HighlightForAction();
                    }

                    UpdateUIForPawn(_battleManager.CurrentPawn);
                }
            }
        }

        if (hoveredPawn != null)
        {
            StopCoroutine(OpenTooltipAfterPause());
            StartCoroutine(OpenTooltipAfterPause());
        }
    }

   public void HandleTileHoverEnd(Tile t)
    {
        charTooltip.Hide();
    
        foreach (Tile highlightedTile in tilesToHighlight)
        {
            highlightedTile.ClearActionHighlight();
        }
        tilesToHighlight.Clear();

        _hoveredTile = null;
    }        
    
    private IEnumerator OpenTooltipAfterPause()
    {
        yield return new WaitForSeconds(.25f);
        ShowTooltipForPawn();
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
        foreach (Pawn p in _battleManager.InitiativeStack)
        {
            // the UI only has space for 7 to look pretty
            if (newChildCount > 7)
                break;

            if (p.IsDead)
            {
                continue;
            }

            GameObject pawnPreview = ObjectPool.Instance.GetPawnPreview();
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
        // UpdateUIForPawn(_currentPawn);
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
        UpdateUIForPawn(_battleManager.CurrentPawn);
    }
    #endregion
}