using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

public class BattleUI : MonoBehaviour
{
    [SerializeField] private Button _endTurnButton;
    [SerializeField] TMP_Text turnText;
    [SerializeField] GameObject turnUI;
    [SerializeField] TMP_Text winLoseText;
    [SerializeField] GameObject winLoseUI;
    [SerializeField] private Button gameOverButton;
    [SerializeField] TMP_Text castleHitPointsUI;
    [SerializeField] TMP_Text hitChanceText;
    [SerializeField] TMP_Text characterNameText;
    [SerializeField] TMP_Text characterMotivatorText;
    [SerializeField] PawnPreview currentPawnPreview;
    [SerializeField] PipStatBar armorBar;
    [SerializeField] PipStatBar healthBar;
    [SerializeField] PipStatBar apBar;
    [SerializeField] PipStatBar motBar;
    [SerializeField] private Transform _initStackParent;
    [SerializeField] private CharacterTooltip charTooltip;
    [SerializeField] private GameObject _instructionsUI;
    [SerializeField] private Button _startBattleButton;
    [SerializeField] private Transform _actionsParent;
    [SerializeField] private GameObject bottomUIObjects;
    [SerializeField] private Transform _healthBarParent;
    [SerializeField] private MiniStatBar _miniStatBarPrefab;
    [SerializeField] private RectTransform _pawnPointer;
    [SerializeField] private Transform _pawnEffectsParent;
    [SerializeField] private Image _pawnEffectLargePrefab;
    [SerializeField] private List<ActionButton> _actionButtons = new(); 
    [SerializeField] private List<InfoLine> _motivationConditionDisplay = new();

    public UnityEvent OnGameOver = new();
    public UnityEvent OnEndTurn = new();
    
    [SerializeField] private PawnEvents _pawnEvents; 

    private Tile _hoveredTile;
    private List<Tile> tilesToHighlight = new();

    // really, shouldn't have selection manager here. This is just here for now to keep 
    // the refactor from becoming out of scope at the time
    [SerializeField] private SelectionManager _selectionManager;

    private void Awake()
    {
        gameOverButton.onClick.AddListener(NotifyGameOver);
        _endTurnButton.onClick.AddListener(NotifyEndTurn);

        Tile.OnTileHoverStart.AddListener(HandleTileHoverStart);
        Tile.OnTileHoverEnd.AddListener(HandleTileHoverEnd);

        _pawnEvents.AddSpawnListener(RegisterPawn);
        _pawnEvents.AddActedListener(HandlePawnActed);
        _pawnEvents.AddKilledListener(HandlePawnKilled);
    }

    private void OnDestroy()
    {
        gameOverButton.onClick.RemoveListener(NotifyGameOver);
        _endTurnButton.onClick.RemoveListener(NotifyEndTurn);

        Tile.OnTileHoverStart.RemoveListener(HandleTileHoverStart);
        Tile.OnTileHoverEnd.RemoveListener(HandleTileHoverEnd);

        _pawnEvents.RemoveSpawnListener(RegisterPawn);
        _pawnEvents.RemoveActedListener(HandlePawnActed);
        _pawnEvents.RemoveKilledListener(HandlePawnKilled);
    }

    #region Events Emitters
    
    private void NotifyGameOver() { OnGameOver.Invoke(); }
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
    
    public void SetTurnUI(int turnNum)
    {
        turnText.text = turnNum.ToString();
    }

    public void HandleBattleResult(BattleManager.BattleResult battleResult)
    {
        turnUI.SetActive(false);
        winLoseUI.SetActive(true);
        bottomUIObjects.SetActive(false);
        winLoseText.text = battleResult == BattleManager.BattleResult.Win ? "Victory!" : "Defeat!" ;
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
        _hoveredTile = targetTile;
        Pawn hoveredPawn = targetTile.GetPawn();

        if (_selectionManager.SelectedTile != null)
        {
            if (_currentPawn != null)
            {
                if (Ability.SelectedAbility != null && targetTile.GetTileDistance(_currentPawn.CurrentTile) <= Ability.SelectedAbility.range)
                {
                    tilesToHighlight.Clear();
                    tilesToHighlight.Add(targetTile);
                    
                    if (Ability.SelectedAbility as WeaponAbilityData != null && ((WeaponAbilityData)Ability.SelectedAbility).attackStyle == WeaponAbilityData.AttackStyle.LShape)
                    {
                        tilesToHighlight.Add(_currentPawn.CurrentTile.GetClockwiseNextTile(targetTile));
                    }

                    foreach (Tile t in tilesToHighlight)
                    {
                        t.HighlightForAction();
                    }

                    UpdateUIForPawn(_currentPawn);
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
        UpdateUIForPawn(_currentPawn);
    }
}
