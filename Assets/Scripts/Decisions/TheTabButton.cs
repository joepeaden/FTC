using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class TheTabButton : MonoBehaviour
{
    private Image image;
    public Sprite defaultSprite;
    public Sprite selectedSprite;

    public Button TheButton => button;
    private Button button;

    private void Awake()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();
    }

    public void SetSelected(bool selected)
    {
        image.sprite = selected ? selectedSprite : defaultSprite;
    }
}
