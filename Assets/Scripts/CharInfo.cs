using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharInfo
{
    public string CharName => _charName;
    private string _charName;
    public bool IsPlayerChar => _isPlayerChar;
    private bool _isPlayerChar;

    public CharInfo(string newName, bool isPlayerCharacter)
    {
        _charName = newName;
        _isPlayerChar = isPlayerCharacter;
    }

}
