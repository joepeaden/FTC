using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class WishlistButton : MonoBehaviour
{
    [Tooltip("The Steam page URL to open when clicked.")]
    [SerializeField] private string steamUrl = "https://store.steampowered.com/app/3133420/Oaths_Against_the_Darkness/";

    private void Awake()
    {
        // Ensure this GameObject has a Button component
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OpenSteamPage);
    }

    private void OpenSteamPage()
    {
        Application.OpenURL(steamUrl);
    }
}