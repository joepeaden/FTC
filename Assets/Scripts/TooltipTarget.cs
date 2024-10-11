using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipManager.Instance.HandleOpenTooltip(gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.Instance.HandleCloseTooltip();
    }

}
