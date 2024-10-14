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

    private Pawn _pawn;


    #region UnityEventFunctions

    private void Update()
    {
        if (_pawn != null)
        {
            Vector3 objScreenPos = Camera.main.WorldToScreenPoint(_pawn.transform.position);
            objScreenPos.y += yOffset;
            GetComponent<RectTransform>().position = objScreenPos;
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

    private void UpdateEffects()
    {
        for (int i = 0; i < _effectIconParent.childCount; i++)
        {
            Destroy(_effectIconParent.GetChild(i).gameObject);
        }

        if (_pawn.IsMotivated)
        {
            Image effectIcon = Instantiate(_effectIconPrefab, _effectIconParent);

            switch (_pawn.CurrentVice)
            {
                case GameCharacter.CharVices.Greed:
                    effectIcon.color = Color.green;
                    break;
                case GameCharacter.CharVices.Glory:
                    effectIcon.color = Color.red;
                    break;
                case GameCharacter.CharVices.Honor:
                    effectIcon.color = Color.blue;
                    break;
            }
            
        }
    }

}
