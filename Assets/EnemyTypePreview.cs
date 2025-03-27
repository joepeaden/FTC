using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnemyTypePreview : MonoBehaviour
{
    [SerializeField] private TMP_Text enemyCount;
    [SerializeField] private PawnPreview preview;

    public void SetData(GameCharacterData previewChar, int count)
    {
        preview.SetData(previewChar.contractDisplaySprite);
        enemyCount.text = "x " + count;
    }
}
