using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This scriptable allows event listeners and invokers to be decoupled from eachother. At least,
/// that's the idea. 
/// </summary>
[CreateAssetMenu(fileName = "PawnEvents", menuName = "MyScriptables/Events/PawnEvents")]
public class PawnEvents : ScriptableObject
{
    #region Pawn Killed
    private UnityEvent<Pawn> OnKilled = new();

    public void AddKilledListener(UnityAction<Pawn> listener)
    {
        OnKilled.AddListener(listener);
    }
        
    public void RemoveKilledListener(UnityAction<Pawn> listener)
    {
        OnKilled.RemoveListener(listener);
    }

    public void EmitKilled(Pawn p)
    {
        OnKilled.Invoke(p);
    }
    #endregion

    #region Pawn Acted
    private UnityEvent<Pawn> OnActed = new();

    public void AddActedListener(UnityAction<Pawn> listener)
    {
        OnActed.AddListener(listener);
    }
        
    public void RemoveActedListener(UnityAction<Pawn> listener)
    {
        OnActed.RemoveListener(listener);
    }

    public void EmitAct(Pawn p)
    {
        OnActed.Invoke(p);
    }
    #endregion

    #region Pawn Spawned
    private UnityEvent<Pawn> OnSpawned = new();

    public void AddSpawnedListener(UnityAction<Pawn> listener)
    {
        OnSpawned.AddListener(listener);
    }
        
    public void RemoveSpawnedListener(UnityAction<Pawn> listener)
    {
        OnSpawned.RemoveListener(listener);
    }

    public void EmitSpawned(Pawn p)
    {
        OnSpawned.Invoke(p);
    }
    #endregion
}