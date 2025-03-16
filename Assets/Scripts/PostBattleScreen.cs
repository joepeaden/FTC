using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostBattleScreen : MonoBehaviour
{
    [SerializeField] private List<PostBattlePawnRow> rows;

    private void OnEnable()
    {
        int i = 0;
        foreach (Pawn p in BattleManager.Instance.PlayerPawns)
        {
            // really the best thing would be to have this list existing and
            // also if we hit this limit, just instantiate more as needed.
            // So hopefully future me isn't too confused about this when a bug
            // pops up.
            // This is a valid excuse to go get taco bell by the way. If you
            // hit this area and figure out this was the bug, you are allowed
            // to go get taco bell. Get a cheesy gordita crunch and a steak
            // chalupa.
            if (i > rows.Count)
            {
                Debug.Log("ALERT: Not enough character rows.");
                break;
            }

            rows[i].SetData(p);
            i++;
        }

        for (; i < rows.Count; i++)
        {
            rows[i].Hide();
        }
    }
}
