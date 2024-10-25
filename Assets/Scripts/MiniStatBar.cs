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

    #region UnityEventFunctions

    private void Awake()
    {
        // create copy so changes don't affect all other effect icons (UI materials are always shared)
        _classIcon.material = new Material(_classIcon.material);

        _originalClassIconScale = _classIcon.transform.localScale;
    }

    private void Update()
    {
        if (_pawn != null)
        {
            Vector3 objScreenPos = Camera.main.WorldToScreenPoint(_pawn.transform.position);
            objScreenPos.y += yOffset;


            // Convert screen position to a position relative to the UI's canvas
            Vector2 uiPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform.parent as RectTransform,
                objScreenPos,
                Camera.main,
                out uiPos);



            GetComponent<RectTransform>().anchoredPosition = uiPos;
        }
    }

    private void OnDestroy()
    {
        _pawn.OnPawnHit.RemoveListener(UpdateBars);
        _pawn.OnEffectUpdate.RemoveListener(UpdateEffects);
    }

    #endregion

    public void SetData(Pawn p)
    {
        _pawn = p;

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

        p.OnPawnHit.AddListener(UpdateBars);
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

    private IEnumerator UpdateShineLocation()
    {
        float currentValue = 0f;
        while (_pawn.IsMotivated)
        {
            currentValue += .01f;
            _classIcon.material.SetFloat("_ShineLocation", currentValue);

            if (currentValue >= 1f)
            {
                yield return new WaitForSeconds(1f);
                currentValue = 0f;
            }

            yield return null;
        }
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
