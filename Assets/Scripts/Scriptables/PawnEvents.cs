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

    #region Pawn Created
    private UnityEvent<Pawn> OnCreated = new();

    public void AddSpawnListener(UnityAction<Pawn> listener)
    {
        OnCreated.AddListener(listener);
    }
        
    public void RemoveSpawnListener(UnityAction<Pawn> listener)
    {
        OnCreated.RemoveListener(listener);
    }

    public void EmitSpawn(Pawn p)
    {
        OnCreated.Invoke(p);
    }
    #endregion
}