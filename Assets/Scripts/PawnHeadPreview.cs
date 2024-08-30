using UnityEngine;
using UnityEngine.UI;

public class PawnHeadPreview : MonoBehaviour
{
    [SerializeField] private Image helmRend;
    [SerializeField] private Image headRend;

    public void SetData(Pawn p)
    {
        headRend.sprite = p.GetFaceSprite();

        Sprite helmSprite = p.GetHelmSprite();
        if (helmSprite != null)
        {
            helmRend.gameObject.SetActive(true);
            helmRend.sprite = helmSprite;
        }
        else
        {
            helmRend.gameObject.SetActive(false);
        }
    }
}
