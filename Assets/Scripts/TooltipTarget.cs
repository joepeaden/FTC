using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// Optional title for simple descriptions
    /// </summary>
    public string displayTitle;
    /// <summary>
    /// An optional string that can be used to describe things that just need a
    /// text description, like stats
    /// </summary>
    public string displayString;

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipManager.Instance.HandleOpenTooltip(gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.Instance.HandleCloseTooltip();
    }

}
