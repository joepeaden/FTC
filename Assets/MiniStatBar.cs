using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniStatBar : MonoBehaviour
{
    [SerializeField] private float yOffset;

    [SerializeField] private StatBar _hpBar;
    [SerializeField] private StatBar _arBar;

    private Pawn _pawn;

    public void SetData(Pawn p)
    {
        _pawn = p;

        UpdateBars();

        p.OnPawnHit.AddListener(UpdateBars);
    }

    private void Update()
    {
        if (_pawn != null)
        {
            Vector3 objScreenPos = Camera.main.WorldToScreenPoint(_pawn.transform.position);
            objScreenPos.y += yOffset;
            GetComponent<RectTransform>().position = objScreenPos;
        }
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

    private void OnDestroy()
    {
        _pawn.OnPawnHit.RemoveListener(UpdateBars);
    }
}
