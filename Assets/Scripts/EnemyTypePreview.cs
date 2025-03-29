using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// For displaying info about a certain type of enemy, like on a contract
/// description for example.
/// </summary>
public class EnemyTypePreview : MonoBehaviour
{
    [SerializeField] private TMP_Text enemyCount;
    [SerializeField] private PawnPreview preview;

    /// <summary>
    /// Will use the contractDisplaySprite from the previewChar.
    /// </summary>
    /// <param name="previewChar"></param>
    /// <param name="count"></param>
    public void SetData(GameCharacterData previewChar, int count)
    {
        preview.SetData(previewChar.contractDisplaySprite);
        enemyCount.text = "x " + count;
    }
}
