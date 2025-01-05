using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance => _instance;
    private static TooltipManager _instance;

    //[SerializeField] private EquipmentTooltip _tooltipPrefab;
    [SerializeField] private EquipmentTooltip _tooltip;

    void Awake()
    {
        _instance = this;

        //_tooltip = Instantiate(_tooltipPrefab);
    }

    public void HandleOpenTooltip(GameObject hoveredGO)
    {
        ItemUI itemUI = hoveredGO.GetComponent<ItemUI>();
        if (itemUI != null)
        {
            _tooltip.SetItem(itemUI.Item);
        }

        ActionButton actionButton = hoveredGO.GetComponent<ActionButton>();
        if (actionButton != null && actionButton.TheAbility.GetData() as ActionData != null)
        {
            _tooltip.SetAction((ActionData)actionButton.TheAbility.GetData());
        }

        EffectIcon effectIcon = hoveredGO.GetComponent<EffectIcon>();
        if (effectIcon != null)
        {
            _tooltip.SetEffect(effectIcon.Effect);
        }
    }

    public void HandleCloseTooltip()
    {
        _tooltip.Hide();
    }
}
