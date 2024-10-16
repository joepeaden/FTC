using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleLogUI : MonoBehaviour
{
    [SerializeField] private BattleLogEntry _logEntryPrefab;
    [SerializeField] private Transform _logParent;

    public static BattleLogUI Instance => _instance;
    private static BattleLogUI _instance;

    private void Awake()
    {
        _instance = this;
    }

    public void AddLogEntry(string newEntryText)
    {
        BattleLogEntry newLog = Instantiate(_logEntryPrefab, _logParent);
        newLog.SetText(newEntryText);
    }
}
