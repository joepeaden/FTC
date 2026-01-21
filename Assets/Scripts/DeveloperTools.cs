using UnityEngine;

public class DeveloperTools : MonoBehaviour
{
    [SerializeField] private AIPlayer _aiPlayer;

#if UNITY_EDITOR
    public void KillEnemyPawns()
    {
        foreach (Pawn p in _aiPlayer.GetEnemyLivingPawns())
        {
            p.Die();
        }
    }
#endif
}
