using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniStatBar : MonoBehaviour
{
    [SerializeField] private float yOffset;

    [SerializeField] private StatBar _hpBar;
    [SerializeField] private StatBar _arBar;

    [SerializeField] private Image _effectIconPrefab;
    [SerializeField] private Transform _effectIconParent;
    [SerializeField] private Image _classIcon;

    private Pawn _pawn;
    private Vector3 _originalClassIconScale;
    private RectTransform _rect;
    private Vector3 _lastPawnPos;

    #region UnityEventFunctions

    private void Awake()
    {
        // create copy so changes don't affect all other effect icons (UI materials are always shared)
        _classIcon.material = new Material(_classIcon.material);

        _originalClassIconScale = _classIcon.transform.localScale;
        _rect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (_pawn != null && _pawn.transform.position != _lastPawnPos)
        {
            UpdatePosition();

            _lastPawnPos = _pawn.transform.position;
        }
    }

    private void OnDestroy()
    {
        _pawn.OnHPChanged.RemoveListener(UpdateBars);
        _pawn.OnEffectUpdate.RemoveListener(UpdateEffects);
    }

    #endregion

    private void UpdatePosition()
    {
        Vector3 objScreenPos = CameraManager.MainCamera.WorldToScreenPoint(_pawn.transform.position);
        objScreenPos.y += yOffset;


        // Convert screen position to a position relative to the UI's canvas
        Vector2 uiPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent as RectTransform,
            objScreenPos,
            CameraManager.MainCamera,
            out uiPos);

        _rect.anchoredPosition = uiPos;
    }

    public void SetData(Pawn p)
    {
        _pawn = p;
        _lastPawnPos = _pawn.transform.position;

        UpdatePosition();

        UpdateBars();

        //switch (_pawn.CurrentVice)
        //{
        //    case GameCharacter.CharVices.Greed:
        //        _classIcon.color = Color.green;
        //        break;
        //    case GameCharacter.CharVices.Glory:
        //        _classIcon.color = Color.red;
        //        break;
        //    case GameCharacter.CharVices.Honor:
        //        _classIcon.color = Color.blue;
        //        break;
        //}

        //_classIcon.material.SetColor("_InnerOutlineColor", _classIcon.color);

        UpdateEffects(p.CurrentEffects);

        p.OnHPChanged.AddListener(UpdateBars);
        p.OnEffectUpdate.AddListener(UpdateEffects);
    }

    private void UpdateBars()
    {
        if (_pawn.IsDead)
        {
            Destroy(gameObject);
            return;
        }

        if (_pawn.ArmorPoints <= 0)
        {
            _arBar.gameObject.SetActive(false);
        }
        else
        {
            _arBar.SetBar(_pawn.MaxArmorPoints, _pawn.ArmorPoints);
        }

        _hpBar.SetBar(_pawn.MaxHitPoints, _pawn.HitPoints);
    }

    private void UpdateEffects(List<EffectData> effects)
    {
        for (int i = 0; i < _effectIconParent.childCount; i++)
        {
            Destroy(_effectIconParent.GetChild(i).gameObject);
        }

        foreach (EffectData effect in effects)
        {
            Image effectIcon = Instantiate(_effectIconPrefab, _effectIconParent);
            effectIcon.sprite = effect.effectSprite;
        }
    }

}
