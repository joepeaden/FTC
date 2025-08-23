using UnityEngine;
using TMPro;

/// <summary>
/// Just for displaying simple lines of text. Has a label, value, and ability to
/// show / hide.
/// </summary>
public class InfoLine : MonoBehaviour
{
    [SerializeField] TMP_Text infoLabel;
    [SerializeField] TMP_Text infoValue;

    public bool isHidden = true;

    private TooltipTarget tooltipTarget;

    private void Awake()
    {
        tooltipTarget = GetComponent<TooltipTarget>();
    }

    public void Hide()
    {
        isHidden = true;
        gameObject.SetActive(false);
    }

    public void SetData(string infoLabelString, string infoValueString, string tooltipDescription = "")
    {
        isHidden = false;

        gameObject.SetActive(true);

        if (infoLabel != null)
        {
            infoLabel.text = infoLabelString;
        }

        infoValue.text = infoValueString;

        if (tooltipDescription != "")
        {
            // set the title of the tooltip
            tooltipTarget.displayTitle = infoValueString;
            // set tooltip description
            tooltipTarget.displayString = tooltipDescription;
        }
    }
}
