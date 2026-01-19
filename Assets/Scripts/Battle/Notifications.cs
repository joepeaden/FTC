using UnityEngine;
using System.Collections.Generic;

public class Notifications : MonoBehaviour
{
    public static Notifications Instance => _instance;
    private static Notifications _instance;
    [SerializeField] private List<TextNotificationStack> _textNotifs;
    private List<(string, Color)> pendingTextNotifs = new();

    private void Awake()
    {
        if (_instance != null)
        {
            Debug.Log("More than one Notifications object!");
            Destroy(this.gameObject);
        }

        _instance = this;
    }
    
    public void AddPendingTextNotification(string str, Color color)
    {
        pendingTextNotifs.Add((str, color));
    }
    
    public void TriggerTextNotification(Vector3 pos)
    {
        foreach (TextNotificationStack txt in _textNotifs)
        {
            if (txt.InUse)
            {
                continue;
            }
            else
            {
                txt.SetData(pos, pendingTextNotifs);
                break;
            }
        }

        pendingTextNotifs.Clear();
    }
}