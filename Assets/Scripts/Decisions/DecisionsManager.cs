using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecisionsManager : MonoBehaviour
{
    [SerializeField] private Transform _panelParent;

    private void OnEnable()
    {
        for (int i = 0; i < _panelParent.childCount; i++)
        {
            DecisionPanel panel =_panelParent.GetChild(i).GetComponent<DecisionPanel>();
            panel.GenerateOption();
        }
    }
}
