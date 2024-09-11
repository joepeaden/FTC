using UnityEngine;
using UnityEngine.UI;

public class PawnHeadPreview : MonoBehaviour
{
    [SerializeField] private Image helmRend;
    [SerializeField] private Image headRend;

    public void SetData(Pawn p)
    {
        headRend.sprite = p.GetFaceSprite();

        ArmorItemData helm = p.GameChar.HelmItem;
        if (helm != null)
        {
            helmRend.gameObject.SetActive(true);
            helmRend.sprite = helm.itemSprite;
        }
        else
        {
            helmRend.gameObject.SetActive(false);
        }
    }
}
