using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ContextMenu : MonoBehaviour
{
    public static ContextMenu Instance => _instance;
    private static ContextMenu _instance;

    private FollowMouse mouseFollow;
    [SerializeField] private ContextButton contextButtonPrefab;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this);
        }

        mouseFollow = GetComponent<FollowMouse>();
    }

    public void SetOptionsAndShow(Dictionary<string, UnityAction> options)
    {
        foreach (KeyValuePair<string, UnityAction> kvp in options)
        {
            string text = kvp.Key;
            UnityAction callback = kvp.Value;

            ContextButton newContextButton = Instantiate(contextButtonPrefab, transform);
            newContextButton.SetData(text, callback);
        }

        mouseFollow.ShouldFollow = false;
    }

    public void Hide()
    {        
        // DESTROY THE CHILDREN MUAHAHAHAHAHAHAHAHAHAHAHAHAHAAHA
        
        // haiku time

        // Mercy can't be had —
        // Though pooling might be better,
        // Children must be crushed.
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        mouseFollow.ShouldFollow = true;
    }
}
