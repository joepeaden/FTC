using UnityEngine;

/// <summary>
/// Singleton instance for opening and closing tooltip. Contains reference
/// to tooltip instance. There should only ever be one of these.
/// </summary>
public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance => _instance;
    private static TooltipManager _instance;

    [SerializeField] private EquipmentTooltip _tooltip;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void HandleOpenTooltip(GameObject hoveredGO)
    {
        ItemUI itemUI = hoveredGO.GetComponent<ItemUI>();
        if (itemUI != null)
        {
            _tooltip.SetItem(itemUI.Item);
            return;
        }
        ActionButton actionButton = hoveredGO.GetComponent<ActionButton>();
        if (actionButton != null)
        {
            _tooltip.SetAction(actionButton.TheAbility.GetData()) ;
            return;
        }
        EffectIcon effectIcon = hoveredGO.GetComponent<EffectIcon>();
        if (effectIcon != null)
        {
            _tooltip.SetEffect(effectIcon.Effect);
            return;
        }

        TooltipTarget tt = hoveredGO.GetComponent<TooltipTarget>();
        if (tt != null)
        {
            _tooltip.SetDescription(tt.displayTitle, tt.displayString);
        }

    }

    public void HandleCloseTooltip()
    {
        _tooltip.Hide();
    }
}
